
RESULTS_DIR := test-results
REPORT_DIR := coverage-report

.PHONY: test report open clean

test:
	rm -rf "$(RESULTS_DIR)"
	dotnet test --collect:'XPlat Code Coverage;Format=cobertura' --results-directory "$(RESULTS_DIR)"

report:
	dotnet tool restore
	rm -rf "$(REPORT_DIR)"
	dotnet tool run reportgenerator -- "-reports:$(RESULTS_DIR)/**/coverage.cobertura.xml" "-targetdir:$(REPORT_DIR)" "-reporttypes:Html"

open:
	open "$(REPORT_DIR)/index.html"

clean:
	rm -rf "$(REPORT_DIR)" "$(RESULTS_DIR)"

