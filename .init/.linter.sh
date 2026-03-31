#!/bin/bash
cd /home/kavia/workspace/code-generation/github-repository-explorer-243913-243927/github_repo_listing_frontend
npm run build
EXIT_CODE=$?
if [ $EXIT_CODE -ne 0 ]; then
   exit 1
fi

