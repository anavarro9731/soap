Param(
   [Parameter(Mandatory = $true)] [string] $appPath,
   [string] $action
   )

function WriteIndexFile {
   @"
<!DOCTYPE html>
<html lang='en'>
   <head>
      <meta charset='utf-8' />
      <!-- Consider mobile layout -->
      <meta name='viewport' content='width=device-width, initial-scale=1' />
      <title>Application</title>
   </head>
   <body>
      <div id='content'></div>
   </body>
   </html>
   <script src='$appPath/app.js'></script>
"@ | out-file -encoding utf8 ../index.html
}

function Build {
      WriteIndexFile

      if (Test-Path "./dist") {
         Remove-Item ./dist -Recurse
      }

      npx parcel build ../index.html --out-dir dist --public-url ./
}

function Run {
      WriteIndexFile

      npx parcel ../index.html --out-dir dist
}


function Main {

      function MoveToChildAppDirectory {

         Set-Location $appPath

      }  

      function CleanAppPath {

         Param([string] $path)

         $path = $path.Trim('/').Trim('\').Replace("\", "/")

         if (-not (Test-Path "$PSScriptRoot/$path")) {
            throw "Directory $path invalid"
         }

         return $path
      }

    try {

      $initialDirectory = $PWD

      $appPath = CleanAppPath $appPath
      
      MoveToChildAppDirectory


      if ($action -eq "build") {
         Build
      } 
      elseif ($action) {
         throw "$action not a valid command"
      }
      else {
         Run
      }
    }

    finally {

        Set-Location $initialDirectory 
   }

}


Main