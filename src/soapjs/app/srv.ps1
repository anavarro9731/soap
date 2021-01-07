param(
    [switch]$UpgradeSoap
)
#kill -name node
Remove-Item -Recurse -Force .parcel-cache
Remove-Item -Recurse -Force dist
if ($UpgradeSoap) {
    yarn upgrade @soap/modules@latest
}
yarn run serve
