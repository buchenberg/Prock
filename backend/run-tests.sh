#!/bin/bash

# Bash script to run tests with coverage
CONFIGURATION="Debug"
FRAMEWORK="net8.0"
COVERAGE=false
WATCH=false
FILTER=""

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -f|--framework)
            FRAMEWORK="$2"
            shift 2
            ;;
        --coverage)
            COVERAGE=true
            shift
            ;;
        --watch)
            WATCH=true
            shift
            ;;
        --filter)
            FILTER="$2"
            shift 2
            ;;
        *)
            echo "Unknown option $1"
            exit 1
            ;;
    esac
done

echo -e "\033[32mRunning Prock Backend Tests\033[0m"
echo -e "\033[33mConfiguration: $CONFIGURATION\033[0m"
echo -e "\033[33mFramework: $FRAMEWORK\033[0m"

# Base test command
TEST_COMMAND="dotnet test backend.Tests/backend.Tests.csproj --configuration $CONFIGURATION --framework $FRAMEWORK --verbosity normal"

# Add filter if specified
if [ ! -z "$FILTER" ]; then
    TEST_COMMAND="$TEST_COMMAND --filter \"$FILTER\""
    echo -e "\033[33mFilter: $FILTER\033[0m"
fi

# Add coverage if requested
if [ "$COVERAGE" = true ]; then
    echo -e "\033[33mCoverage: Enabled\033[0m"
    TEST_COMMAND="$TEST_COMMAND --collect:\"XPlat Code Coverage\" --results-directory TestResults"
    
    # Run tests with coverage
    eval $TEST_COMMAND
    
    # Generate coverage report
    if [ -d "TestResults" ]; then
        echo -e "\033[32mGenerating coverage report...\033[0m"
        COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" | head -1)
        if [ ! -z "$COVERAGE_FILE" ]; then
            dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources
            reportgenerator -reports:$COVERAGE_FILE -targetdir:TestResults/CoverageReport -reporttypes:"Html;TextSummary"
            echo -e "\033[32mCoverage report generated at: TestResults/CoverageReport/index.html\033[0m"
        fi
    fi
elif [ "$WATCH" = true ]; then
    echo -e "\033[33mWatch mode: Enabled\033[0m"
    TEST_COMMAND="$TEST_COMMAND --watch"
    eval $TEST_COMMAND
else
    # Run tests normally
    eval $TEST_COMMAND
fi

echo -e "\033[32mTest execution completed!\033[0m"

