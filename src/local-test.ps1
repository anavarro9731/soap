.\pwsh-bootstrap.ps1
git push
[System.Console]::ResetColor()
git add -A 
git commit -a -m "build test"
Run -PrepareNewVersion 

Run -PackAndPublish `
       -nugetApiKey "j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia" `
       -azureDevopsPat "j35ssqoabmwviu7du4yin6lmw3l2nc4okz37tcdmpirl3ftgyiia" `
       -azClientId "fbff5a19-e7a7-49a2-829c-ad415b507577" `
       -azClientSecret "34ME15tmte0Hm8x6oXuV_deo~bgzJqK~H-" `
       -azTenantId "f8d686ac-a87f-4d1c-bbcf-d08873871dcd"
