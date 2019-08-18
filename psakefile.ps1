Task Publish -Depends Pack {
    Exec { docker login docker.io --username=tiksn }
    foreach ($VersionTag in $script:VersionTags) {
        $localTag = ($script:imageName + ":" + $VersionTag)
        $remoteTag = ("docker.io/" + $localTag)
        Exec { docker tag $localTag $remoteTag }
        Exec { docker push $remoteTag }
    }
}

Task Pack -Depends Build, EstimateVersions {
    $src = (Resolve-Path ".\src\").Path
    $tagsArguments = @()
    foreach ($VersionTag in $VersionTags) {
        $tagsArguments += "-t"
        $tagsArguments += ($script:imageName + ":" + $VersionTag)
    }

    Exec { docker build -f Dockerfile $src $tagsArguments }
}

Task EstimateVersions {
    $script:VersionTags = @()

    if ($Latest) {
        $script:VersionTags += 'latest'
    }

    if (!!($Version)) {
        $Version = [Version]$Version

        Assert ($Version.Revision -eq -1) "Version should be formatted as Major.Minor.Patch like 1.2.3"
        Assert ($Version.Build -ne -1) "Version should be formatted as Major.Minor.Patch like 1.2.3"

        $Version = $Version.ToString()
        $script:VersionTags += $Version
    }

    Assert $script:VersionTags "No version parameter (latest or specific version) is passed."
}

Task Build -Depends TranspileModels {
    $script:publishFolder = Join-Path -Path $script:trashFolder -ChildPath "publish"

    New-Item -Path $script:publishFolder -ItemType Directory | Out-Null
    $project = Resolve-Path ".\src\WebAPI\WebAPI.csproj"
    $project = $project.Path
    Exec { dotnet publish $project --output $script:publishFolder }
}

Task TranspileModels -Depends Init, Clean {
}

Task Clean -Depends Init {
}

Task Init {
    $date = Get-Date
    $ticks = $date.Ticks
    $script:imageName = "tiksn/lionize-habitica-task-provider-service"
    $script:trashFolder = Join-Path -Path . -ChildPath ".trash"
    $script:trashFolder = Join-Path -Path $script:trashFolder -ChildPath $ticks.ToString("D19")
    New-Item -Path $script:trashFolder -ItemType Directory | Out-Null
    $script:trashFolder = Resolve-Path -Path $script:trashFolder
}