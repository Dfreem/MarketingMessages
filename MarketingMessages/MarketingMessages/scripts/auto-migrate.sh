#!/bin/bash
# auto-migrate.sh
# Automates EF Core migrations with context selection and idempotent script generation.

set -e

# Step 1: Get migration name
MIGRATION_NAME=$1
if [ -z "$MIGRATION_NAME" ]; then
    echo "⚠ Please provide a migration name."
    echo "Example: ./auto-migrate.sh AddCustomerTable"
    exit 1
fi

# Step 2: Find all DbContexts in the project
CONTEXTS=$(grep -R "DbContext" -n --include="*.cs" | grep "class " | awk '{print $3}' | sed 's/:.*//' | sort -u)

if [ -z "$CONTEXTS" ]; then
    echo "❌ No DbContext found in the project."
    exit 1
fi

# Step 3: Prompt for context if more than one
if [ $(echo "$CONTEXTS" | wc -l) -gt 1 ]; then
    echo "🔍 Multiple DbContexts found. Please select one:"
    select CONTEXT in $CONTEXTS; do
        if [ -n "$CONTEXT" ]; then
            break
        fi
    done
else
    CONTEXT=$CONTEXTS
    echo "✅ Using DbContext: $CONTEXT"
fi

# Step 4: Add migration
echo "📦 Adding migration '$MIGRATION_NAME' for context '$CONTEXT'..."
dotnet ef migrations add "$MIGRATION_NAME" --context "$CONTEXT"

# Step 5: Update database
echo "🗄 Updating database for context '$CONTEXT'..."
dotnet ef database update --context "$CONTEXT"

# Step 6: Generate idempotent SQL script
SQL_FILE="Migrations/${MIGRATION_NAME}_$(date +%Y%m%d_%H%M%S).sql"
echo "📝 Generating idempotent SQL script: $SQL_FILE"
dotnet ef migrations script --idempotent --context "$CONTEXT" -o "$SQL_FILE"

echo "✅ Migration complete."
echo "   Migration name: $MIGRATION_NAME"
echo "   Context: $CONTEXT"
echo "   SQL script saved to: $SQL_FILE"
