# First clean the Release target.
xbuild MK.Solar.csproj /p:Configuration=Release /t:Clean

# Now build the project, using the Release target.
xbuild MK.Solar.csproj /p:Configuration=Release /t:PackageForAndroid

# At this point there is only the unsigned APK - sign it.
# The script will pause here as jarsigner prompts for the password.
# It is possible to provide they keystore password for jarsigner.exe by adding an extra command line parameter -storepass, for example
#    -storepass <MY_SECRET_PASSWORD>
# If this script is to be checked in to source code control then it is not recommended to include the password as part of this script.

jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ../../signing/tigerspike-debug.keystore -signedjar ./bin/Release/mk.solar-signed.apk ./bin/Release/mk.solar.apk tigerspikeDebugKey

# Now zipalign it.  The -v parameter tells zipalign to verify the APK afterwards.
zipalign -f -v 4 bin/Release/mk.solar-signed.apk ../../calabash/MKSolarAutomation/mk.solar.apk

(cd ../../calabash/MKSolarAutomation/ && calabash-android run mk.solar.apk)
