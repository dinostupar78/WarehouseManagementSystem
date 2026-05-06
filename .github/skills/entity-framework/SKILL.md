---
name: entity-framework
description: "Handles Entity Framework Core tasks such as modifying model classes, generating migrations, updating database schema, and managing EF operations in WarehouseManagementSystem."
applyTo: "**/*.cs"
---

# Entity Framework Skill

This skill is designed to assist with Entity Framework Core operations in the WarehouseManagementSystem project. It can be invoked when you need to:

- Modify Entity Framework model classes (e.g., adding properties, changing relationships)
- Generate new database migrations
- Apply migrations to update the database
- Handle database schema changes
- Manage Entity Framework Core configurations

## Usage

When working with Entity Framework classes or needing to update the database schema, this skill will:

1. Analyze the current model changes
2. Generate appropriate migration files
3. Apply migrations to the database
4. Validate the changes

## Supported Operations

- Adding/modifying properties in model classes
- Creating new entity classes
- Updating relationships between entities
- Generating migrations with `dotnet ef migrations add`
- Applying migrations with `dotnet ef database update`
- Removing migrations if needed
- Handling migration conflicts

## Safety Rules

- Never remove migrations unless explicitly requested
- Never perform destructive schema changes without confirmation
- Preserve existing relationships and constraints
- Review generated migrations before applying them
- Do not modify production connection strings

## Project Context

This skill is specifically configured for the WarehouseManagementSystem project structure:
- Model classes in `WarehouseManagementSystem.Model/`
- Database context in `WarehouseManagementSystem.DAL/`
- Migrations folder in `WarehouseManagementSystem.DAL/Migrations/`

Use this skill whenever you make changes to EF model classes or need to synchronize the database schema.