@echo off
mkdir %~dp0dlr
ruby %~dp0..\Scripts\generate_dlrjs.rb
copy %~dp0..\Scripts\dlr.js .
%~dp0..\..\..\Bin\"Silverlight Release"\Chiron.exe /b:index.html
