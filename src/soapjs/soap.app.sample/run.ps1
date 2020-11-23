kill -name node
Remove-Item -Recurse -Force .parcel-cache
npm run-script run
