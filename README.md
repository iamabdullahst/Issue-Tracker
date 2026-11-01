# Issue Tracker

A full-stack practice app for creating and closing issues.

## Stack

- ASP.NET Core (.NET 10), Entity Framework Core, and SQLite
- React and Vite
- xUnit API tests
- GitHub Actions checks for the API and web app

## Run locally

In one Terminal, run `cd IssueTracker.Api` followed by `dotnet run --urls http://localhost:5050`.

In another Terminal, run `cd issue-tracker-web`, `npm ci`, and `npm run dev`.

Open http://localhost:5173.

## Checks

From the project root, run `dotnet test IssueTracker.Tests/IssueTracker.Tests.csproj`.

From `issue-tracker-web`, run `npm run lint` and `npm run build`.

React requests `/issues`, which Vite forwards to the .NET API during development. The API saves issues in a local SQLite file that Git ignores. The API tests use separate temporary databases.
