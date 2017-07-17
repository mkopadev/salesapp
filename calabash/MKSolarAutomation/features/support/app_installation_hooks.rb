require 'calabash-android/management/app_installation'

AfterConfiguration do |config|
	FeatureNameMemory.feature_name = nil
end

def resign_and_reinstall_app
  app_path = ENV["APP_PATH"]
  ENV["TEST_APP_PATH"] = test_server_path(ENV["APP_PATH"])
  system "calabash-android build %s" % [app_path]
  system "calabash-android resign %s" % [app_path]

  uninstall_apps
  install_app(ENV['TEST_APP_PATH'])
  install_app(ENV['APP_PATH'])
end

Before do |scenario|
  @scenario_is_outline = (scenario.class == Cucumber::Ast::OutlineTable::ExampleRow)
  if @scenario_is_outline 
    scenario = scenario.scenario_outline 
  end 

  feature_name = scenario.feature.title
  if FeatureNameMemory.feature_name != feature_name \
      or ENV["RESET_BETWEEN_SCENARIOS"] == "1"
    if ENV["RESET_BETWEEN_SCENARIOS"] == "1"
      log "New scenario - reinstalling apps"
    else
      log "First scenario in feature - reinstalling apps"
    end

    resign_and_reinstall_app

    FeatureNameMemory.feature_name = feature_name
	FeatureNameMemory.invocation = 1
  else
    FeatureNameMemory.invocation += 1
  end
end

FeatureNameMemory = Class.new
class << FeatureNameMemory
  @feature_name = nil
  attr_accessor :feature_name, :invocation
end
