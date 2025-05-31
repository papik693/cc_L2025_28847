#!/bin/bash

if [ -z "$1" ]; then
    echo "Please provide the function app name as parameter"
    echo "Usage: ./deploy.sh <function-app-name> [resource-group-name]"
    exit 1
fi

FUNCTION_APP_NAME=$1
RESOURCE_GROUP=${2:-cdv} 

cd function_app

# Clean up previous build
rm -rf .build
mkdir -p .build

# Copy function files to build directory
cp -r api .build/
cp requirements.txt .build/

# Create and activate virtual environment
python3 -m venv .build/.venv
source .build/.venv/bin/activate

# Install dependencies
cd .build
pip install --target=. -r requirements.txt

# Create deployment package
rm -f ../../function_app.zip
zip -r ../../function_app.zip .

# Clean up and deploy
cd ../..
echo "Deploying to Azure..."
az functionapp deployment source config-zip \
  -g $RESOURCE_GROUP \
  -n $FUNCTION_APP_NAME \
  --src function_app.zip

# Clean up build artifacts
rm -f function_app.zip
rm -rf function_app/.build 