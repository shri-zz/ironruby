# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{activerecord-sqlserver-adapter}
  s.version = "2.2.19"

  s.required_rubygems_version = Gem::Requirement.new(">= 1.2") if s.respond_to? :required_rubygems_version=
  s.authors = ["Ken Collins, Murray Steele, Shawn Balestracci, Joe Rafaniello, Tom Ward"]
  s.date = %q{2009-06-17}
  s.description = %q{SQL Server 2000, 2005 and 2008 Adapter For Rails.}
  s.email = %q{ken@metaskills.net}
  s.extra_rdoc_files = ["CHANGELOG", "lib/active_record/connection_adapters/sqlserver_adapter.rb", "lib/activerecord-sqlserver-adapter.rb", "lib/core_ext/active_record.rb", "lib/core_ext/dbi.rb", "README.rdoc", "tasks/sqlserver.rake"]
  s.files = ["CHANGELOG", "lib/active_record/connection_adapters/sqlserver_adapter.rb", "lib/activerecord-sqlserver-adapter.rb", "lib/core_ext/active_record.rb", "lib/core_ext/dbi.rb", "Manifest", "MIT-LICENSE", "Rakefile", "README.rdoc", "RUNNING_UNIT_TESTS", "tasks/sqlserver.rake", "test/cases/aaaa_create_tables_test_sqlserver.rb", "test/cases/adapter_test_sqlserver.rb", "test/cases/attribute_methods_test_sqlserver.rb", "test/cases/basics_test_sqlserver.rb", "test/cases/calculations_test_sqlserver.rb", "test/cases/column_test_sqlserver.rb", "test/cases/connection_test_sqlserver.rb", "test/cases/eager_association_test_sqlserver.rb", "test/cases/execute_procedure_test_sqlserver.rb", "test/cases/inheritance_test_sqlserver.rb", "test/cases/method_scoping_test_sqlserver.rb", "test/cases/migration_test_sqlserver.rb", "test/cases/offset_and_limit_test_sqlserver.rb", "test/cases/pessimistic_locking_test_sqlserver.rb", "test/cases/query_cache_test_sqlserver.rb", "test/cases/schema_dumper_test_sqlserver.rb", "test/cases/specific_schema_test_sqlserver.rb", "test/cases/sqlserver_helper.rb", "test/cases/table_name_test_sqlserver.rb", "test/cases/transaction_test_sqlserver.rb", "test/cases/unicode_test_sqlserver.rb", "test/connections/native_sqlserver/connection.rb", "test/connections/native_sqlserver_odbc/connection.rb", "test/migrations/transaction_table/1_table_will_never_be_created.rb", "test/schema/sqlserver_specific_schema.rb"]
  s.homepage = %q{http://github.com/rails-sqlserver}
  s.rdoc_options = ["--line-numbers", "--inline-source", "--title", "Activerecord-sqlserver-adapter", "--main", "README.rdoc"]
  s.require_paths = ["lib"]
  s.rubyforge_project = %q{arsqlserver}
  s.rubygems_version = %q{1.3.5}
  s.summary = %q{SQL Server 2000, 2005 and 2008 Adapter For Rails.}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 2

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
      s.add_runtime_dependency(%q<dbi>, ["= 0.4.1"])
      s.add_runtime_dependency(%q<dbd-odbc>, ["= 0.2.4"])
    else
      s.add_dependency(%q<dbi>, ["= 0.4.1"])
      s.add_dependency(%q<dbd-odbc>, ["= 0.2.4"])
    end
  else
    s.add_dependency(%q<dbi>, ["= 0.4.1"])
    s.add_dependency(%q<dbd-odbc>, ["= 0.2.4"])
  end
end
