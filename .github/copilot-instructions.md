# Copilot Instructions

## Project Guidelines
- The user wants all code comments and XML documentation to be written in English.
- When requesting documentation improvements, prioritize `.md` documentation files (docs) over XML documentation in code, unless specified otherwise.
- Documentation intended to guide users on project structure should be intentionally prominent, and future templates are planned to complement that guidance.

## Authorization Guidelines
- In FastSharp, authorization is integrated flexibly as the configuration exposes `RouteHandlerBuilder`, allowing the chaining of native Minimal API methods (e.g., `RequireAuthorization`).