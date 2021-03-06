/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

#if CLR2
namespace Microsoft.Scripting.Ast.Compiler {
#else
namespace System.Linq.Expressions.Compiler {
#endif

    /// <summary>
    /// Dynamic Language Runtime Compiler.
    /// This part compiles lambdas.
    /// </summary>
    partial class LambdaCompiler {
        private static int _Counter;

        internal void EmitConstantArray<T>(T[] array) {
            // Emit as runtime constant if possible
            // if not, emit into IL
            if (_method is DynamicMethod) {
                EmitConstant(array, typeof(T[]));
            } else if(_typeBuilder != null) {
                // store into field in our type builder, we will initialize
                // the value only once.
                FieldBuilder fb = CreateStaticField("ConstantArray", typeof(T[]));
                Label l = _ilg.DefineLabel();
                _ilg.Emit(OpCodes.Ldsfld, fb);
                _ilg.Emit(OpCodes.Ldnull);
                _ilg.Emit(OpCodes.Bne_Un, l);
                _ilg.EmitArray(array);
                _ilg.Emit(OpCodes.Stsfld, fb);
                _ilg.MarkLabel(l);
                _ilg.Emit(OpCodes.Ldsfld, fb);
            } else { 
                _ilg.EmitArray(array);
            }
        }

        private void EmitClosureCreation(LambdaCompiler inner) {
            bool closure = inner._scope.NeedsClosure;
            bool boundConstants = inner._boundConstants.Count > 0;

            if (!closure && !boundConstants) {
                _ilg.EmitNull();
                return;
            }

            // new Closure(constantPool, currentHoistedLocals)
            if (boundConstants) {
                _boundConstants.EmitConstant(this, inner._boundConstants.ToArray(), typeof(object[]));
            } else {
                _ilg.EmitNull();
            }
            if (closure) {
                _scope.EmitGet(_scope.NearestHoistedLocals.SelfVariable);
            } else {
                _ilg.EmitNull();
            }
            _ilg.EmitNew(typeof(Closure).GetConstructor(new Type[] { typeof(object[]), typeof(object[]) }));
        }

        /// <summary>
        /// Emits code which creates new instance of the delegateType delegate.
        /// 
        /// Since the delegate is getting closed over the "Closure" argument, this
        /// cannot be used with virtual/instance methods (inner must be static method)
        /// </summary>
        private void EmitDelegateConstruction(LambdaCompiler inner) {
            Type delegateType = inner._lambda.Type;
            DynamicMethod dynamicMethod = inner._method as DynamicMethod;
            if (dynamicMethod != null) {
                // dynamicMethod.CreateDelegate(delegateType, closure)
                _boundConstants.EmitConstant(this, dynamicMethod, typeof(DynamicMethod));
                _ilg.EmitType(delegateType);
                EmitClosureCreation(inner);
                _ilg.Emit(OpCodes.Callvirt, typeof(DynamicMethod).GetMethod("CreateDelegate", new Type[] { typeof(Type), typeof(object) }));
                _ilg.Emit(OpCodes.Castclass, delegateType);
            } else {
                // new DelegateType(closure)
                EmitClosureCreation(inner);
                _ilg.Emit(OpCodes.Ldftn, (MethodInfo)inner._method);
                _ilg.Emit(OpCodes.Newobj, (ConstructorInfo)(delegateType.GetMember(".ctor")[0]));
            }
        }

        /// <summary>
        /// Emits a delegate to the method generated for the LambdaExpression.
        /// May end up creating a wrapper to match the requested delegate type.
        /// </summary>
        /// <param name="lambda">Lambda for which to generate a delegate</param>
        /// 
        private void EmitDelegateConstruction(LambdaExpression lambda) {
            // 1. Create the new compiler
            LambdaCompiler impl;
            if (_method is DynamicMethod) {
                impl = new LambdaCompiler(_tree, lambda);
            } else {
                // When the lambda does not have a name or the name is empty, generate a unique name for it.
                string name = String.IsNullOrEmpty(lambda.Name) ? GetUniqueMethodName() : lambda.Name;
                MethodBuilder mb = _typeBuilder.DefineMethod(name, MethodAttributes.Private | MethodAttributes.Static);
                impl = new LambdaCompiler(_tree, lambda, mb);
            }

            // 2. emit the lambda
            // Since additional ILs are always emitted after the lambda's body, should not emit with tail call optimization.
            impl.EmitLambdaBody(_scope, false, CompilationFlags.EmitAsNoTail);

            // 3. emit the delegate creation in the outer lambda
            EmitDelegateConstruction(impl);
        }

        private static Type[] GetParameterTypes(LambdaExpression lambda) {
            return lambda.Parameters.Map(p => p.IsByRef ? p.Type.MakeByRefType() : p.Type);
        }

        private static string GetUniqueMethodName() {
            return "<ExpressionCompilerImplementationDetails>{" + Interlocked.Increment(ref _Counter) + "}lambda_method";
        }

        private void EmitLambdaBody() {
            // The lambda body is the "last" expression of the lambda
            CompilationFlags tailCallFlag = _lambda.TailCall ? CompilationFlags.EmitAsTail : CompilationFlags.EmitAsNoTail;
            EmitLambdaBody(null, false, tailCallFlag);
        }

        /// <summary>
        /// Emits the lambda body. If inlined, the parameters should already be
        /// pushed onto the IL stack.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        /// <param name="inlined">true if the lambda is inlined; false otherwise.</param>
        /// <param name="flags">
        /// The emum to specify if the lambda is compiled with the tail call optimization. 
        /// </param>
        private void EmitLambdaBody(CompilerScope parent, bool inlined, CompilationFlags flags) {
            _scope.Enter(this, parent);

            if (inlined) {
                // The arguments were already pushed onto the IL stack.
                // Store them into locals, popping in reverse order.
                //
                // If any arguments were ByRef, the address is on the stack and
                // we'll be storing it into the variable, which has a ref type.
                for (int i = _lambda.Parameters.Count - 1; i >= 0; i--) {
                    _scope.EmitSet(_lambda.Parameters[i]);
                }
            }

            // Need to emit the expression start for the lambda body
            flags = UpdateEmitExpressionStartFlag(flags, CompilationFlags.EmitExpressionStart);
            if (_lambda.ReturnType == typeof(void)) {
                EmitExpressionAsVoid(_lambda.Body, flags);
            } else {
                EmitExpression(_lambda.Body, flags);
            }

            // Return must be the last instruction in a CLI method.
            // But if we're inlining the lambda, we want to leave the return
            // value on the IL stack.
            if (!inlined) {
                _ilg.Emit(OpCodes.Ret);
            }

            _scope.Exit();

            // Validate labels
            Debug.Assert(_labelBlock.Parent == null && _labelBlock.Kind == LabelScopeKind.Lambda);
            foreach (LabelInfo label in _labelInfo.Values) {
                label.ValidateFinish();
            }
        }
    }
}
