param(
    [switch]$UpgradeSoap
)
#kill -name node
Remove-Item -Recurse -Force .parcel-cache #v2
Remove-Item -Recurse -Force .cache #v1
Remove-Item -Recurse -Force dist
if ($UpgradeSoap) {
    yarn upgrade @soap/modules@latest
}
yarn run serve
