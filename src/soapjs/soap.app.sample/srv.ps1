kill -name node
Remove-Item -Recurse -Force .parcel-cache
yarn upgrade @soap/modules
yarn run serve
