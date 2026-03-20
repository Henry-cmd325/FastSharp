# Copilot Instructions

## Project Guidelines
- The user wants all code comments and XML documentation to be written in English.
- When requesting documentation improvements, prioritize `.md` documentation files (docs) over XML documentation in code, unless specified otherwise.

## Authorization Guidelines
- In FastSharp, authorization is integrated flexibly as the configuration exposes `RouteHandlerBuilder`, allowing the chaining of native Minimal API methods (e.g., `RequireAuthorization`).