#!/bin/bash

set -e

if [ ${INPUT_PACKAGE_TYPE} == 'madcrewvr' ]
then
  packagepath=${INPUT_PACKAGE_DIRECTORY_PATH}/MadCrewPackage
elif [ ${INPUT_PACKAGE_TYPE} == 'steamvr' ]
then
  packagepath=${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVRPackage
fi

cat << EOS | sed -i '1r /dev/stdin' $packagepath/CHANGELOG.md

## [${INPUT_RELEASE_VERSION##v}] - $(date "+%Y-%m-%d")

${INPUT_RELEASE_SUMMARY}

$(echo "${INPUT_RELEASE_BODY}" | sed 's/^#/\#\#/')
EOS
cat $packagepath/package.json | jq -Mr '. | .version = "'"${INPUT_RELEASE_VERSION##v}"'"' > /tmp/package.json
mv /tmp/package.json $packagepath/package.json

if [ ${INPUT_PACKAGE_TYPE} == 'madcrewvr' ]
then
  /bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/MadCrew.VR $packagepath/MadCrew.VR
  /bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/MadCrew.VR.meta $packagepath/MadCrew.VR.meta
elif [ ${INPUT_PACKAGE_TYPE} == 'steamvr' ]
then
  /bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVR $packagepath/SteamVR
  /bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVR.meta $packagepath/SteamVR.meta
  /bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVR_Input $packagepath/SteamVR_Input
  /bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVR_Input.meta $packagepath/SteamVR_Input.meta
#/bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVR_Resources $packagepath/SteamVR_Resources
#/bin/cp -r ${INPUT_PACKAGE_DIRECTORY_PATH}/SteamVR_Resources.meta $packagepath/SteamVR_Resources.meta
fi

#if [ -z "${INPUT_NPM_REGISTRY_URL}" ]; then
#    INPUT_NPM_REGISTRY_URL=$(cat .npmrc | sed 's/^registry=//')
#    echo $(cat .npmrc | grep '^registry=' | sed 's/^registry=https://')'/:_authToken="'${INPUT_NPM_AUTH_TOKEN}'"' >> ~/.npmrc
#else
#    echo $(echo -n "${INPUT_NPM_REGISTRY_URL}" | sed 's/^https://')'/:_authToken="'${INPUT_NPM_AUTH_TOKEN}'"' >> ~/.npmrc
#fi
echo "//npm.pkg.github.com/:_authToken=${INPUT_NPM_AUTH_TOKEN}" > .npmrc
echo "//npm.pkg.github.com/:always-auth=true" >> .npmrc
echo "registry=https://npm.pkg.github.com/madcrewteam" >> .npmrc
echo "@madcrewteam:registry=https://npm.pkg.github.com" >> .npmrc
echo "@madcrewteam:registry=https://npm.pkg.github.com" >> .npmrc

npm publish --tag latest --registry ${INPUT_NPM_REGISTRY_URL} $packagepath

git config --global user.email "github-actions@example.com"
git config --global user.name "GitHub Actions"
git config lfs.locksverify false
git checkout -b "temporary-$(date '+%Y%m%d%H%M%S')"
git add $packagepath/package.json
git add $packagepath/CHANGELOG.md
git commit -m ":up: Bump up version: ${INPUT_RELEASE_VERSION}"
git pull "https://${INPUT_GITHUB_ACTOR}}:${INPUT_GITHUB_TOKEN}@github.com/${INPUT_GITHUB_REPOSITORY}.git" HEAD:${INPUT_RELEASE_BRANCH}
git push --no-verify "https://${INPUT_GITHUB_ACTOR}}:${INPUT_GITHUB_TOKEN}@github.com/${INPUT_GITHUB_REPOSITORY}.git" HEAD:${INPUT_RELEASE_BRANCH}
git tag -d ${INPUT_RELEASE_VERSION}
git tag ${INPUT_RELEASE_VERSION}
git push --tags --force
