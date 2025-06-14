# Save this as change_dates.sh or run line by line in Git Bash

# Enter your commit hashes here (space-separated):
COMMIT_HASHES="dcd8545e694ae3ef11997bafd0ea0d45c83ccedd 598e3b8ba4322542d6e5ec063eb6f760f1f4acbd" # Replace with your hashes

git filter-branch -f --env-filter '
for COMMIT in $COMMIT_HASHES; do
  if [ "$GIT_COMMIT" = "$COMMIT" ]; then
    export GIT_AUTHOR_DATE="$(date -R)"
    export GIT_COMMITTER_DATE="$(date -R)"
  fi
done

' -- --all
