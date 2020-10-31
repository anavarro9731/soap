Param(
   [Parameter(Mandatory = $true)]
   [string] $appPath
   )

function Main {

   function MoveToReactComponentsRootDirectory {

      Set-Location $PSScriptRoot

   }  

   function CleanComponentPath {

      Param([string] $path)

      $path = $path.Trim('/').Trim('\').Replace("\", "/")

      if (-not (Test-Path "$PSScriptRoot/$path")) {
         throw "Directory $path invalid"
      }

      return $path
   }

    try {

      $initialDirectory = $PWD

      $appPath = CleanComponentPath $appPath
      
      MoveToReactComponentsRootDirectory

@"
<!DOCTYPE html>
<html lang='en'>
   <head>
      <meta charset='utf-8' />
      <!-- Consider mobile layout -->
      <meta name='viewport' content='width=device-width, initial-scale=1' />
      <title>Playground</title>
   </head>
   <body>
      <div id='content'></div>
   </body>
   </html>
   <script src='$appPath/playground.js'></script>
"@ | out-file -encoding utf8 ./index.html

      npx parcel ./index.html

    }
    finally {

        Set-Location $initialDirectory 
   }
}

Main