---
name: test
description: Add, run, and diagnose tests for Perigon.Modules using TUnit, Microsoft.Testing.Platform, and Aspire.Hosting.Testing. Use for unit tests, module API integration tests, AppHost test fixtures, authenticated HttpClient setup, resource readiness, test database lifecycle, or focused regression verification.
---

# Module Testing

Use the root `global.json` test runner settings. This repository uses TUnit on Microsoft.Testing.Platform, not the VSTest command model.

## Choose the smallest useful level

- Use a unit test for pure validation, mapping, calculation, or domain behavior that does not need the distributed app.
- Use `test/ApiTest` for routing, authorization, serialization, Manager/EF behavior, migrations, initialization, or multi-service integration.
- Add a package-structure check when the regression concerns `metadata.json` or zip contents rather than runtime behavior.

Do not start Aspire for a documentation, skill, or static structure change.

## Follow the existing Aspire fixture

`GlobalHooks` creates `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`, sets `ASPIRE_ENVIRONMENT=Testing`, configures logging/resilience, and starts one application for the test session. `TestHttpClientData` waits for `AdminService`, creates its HttpClient, authenticates, and supplies the bearer token.

Reuse that session fixture instead of starting an AppHost per test. Wait for the specific resource state before sending requests. Keep each test independent at the data level and avoid relying on execution order; the assembly-level retry must not hide deterministic failures.

The current cleanup path connects with Npgsql and drops `Perigon.Modules_test`. Treat it as PostgreSQL-specific: before running or changing database-provider tests, verify AppHost configuration, container runtime, database name, and cleanup safety. Never point this fixture at a non-test database.

## Write tests

- Name tests after observable behavior and expected result.
- Arrange only the minimum data needed; use unique values when constraints or retries can collide.
- Assert status code first, then the response contract and persisted/visible outcome.
- Cover authentication, authorization/tenant boundaries, validation, not-found/conflict paths, and destructive operations when relevant.
- Prefer public HTTP behavior for module integration tests; test Manager internals directly only when the behavior cannot be observed through the contract.
- Clean up test-created state or design data to be isolated within the test database.

Do not leave meaningful assertions commented out. A placeholder test that always passes is not regression coverage.

## Run and filter

From the repository root, list discovered tests with:

```powershell
dotnet test --project test/ApiTest/ApiTest.csproj --list-tests
```

Run the same project for the full integration suite. For focused selection, use TUnit/MTP `--treenode-filter` syntax rather than VSTest `--filter`. Check the installed runner's help before composing a complex expression.

## Diagnose failures

Preserve the first meaningful exception and separate these categories:

- compile or test-discovery failure;
- Docker/Podman, DCP, port, image, or resource-readiness failure;
- migration, seed, authentication, or provider-specific cleanup failure;
- actual endpoint/business regression;
- cleanup-only failure after assertions passed.

Use resource logs and state only as needed. Do not weaken assertions, increase retries, or swallow initialization exceptions to make the suite green.
