Feature: Login feature

  Scenario: As a user I can launch the app
    Then I see "Hello World, Click Me!"
    And I don't see "Nonexistent text"

  Scenario: As a developer I want to be sure that tests fail appropriately
    Then I see "This test will fail"