load('jsmake.dotnet.DotNetUtils.js');

var fs = jsmake.Fs;
var utils = jsmake.Utils;
var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

var version;

task('default', 'build');

task('version', function () {
	version = JSON.parse(fs.readFile('version.json'));
});

task('dependencies', function () {
	var pkgs = fs.createScanner('src').include('**/packages.config').scan();
	dotnet.downloadNuGetPackages(pkgs, 'lib');
});

task('assemblyinfo', 'version', function () {
	dotnet.writeAssemblyInfo('src/SharedAssemblyInfo.cs', {
		AssemblyTitle: 'ADOUtils',
		AssemblyProduct: 'ADOUtils',
		AssemblyDescription: 'ADO.NET utilities',
		AssemblyCopyright: 'Copyright © Gian Marco Gherardi ' + new Date().getFullYear(),
		AssemblyTrademark: '',
		AssemblyCompany: 'Gian Marco Gherardi',
		AssemblyConfiguration: '', // Probably a good place to put Git SHA1 and build date
		AssemblyVersion: [ version.major, version.minor, version.build, 0 ].join('.'),
		AssemblyFileVersion: [ version.major, version.minor, version.build, 0 ].join('.'),
		AssemblyInformationalVersion: [ version.major, version.minor, version.build, 0 ].join('.')
	});
});

task('build', [ 'dependencies', 'assemblyinfo' ], function () {
	dotnet.runMSBuild('src/ADOUtils.sln', [ 'Clean', 'Rebuild' ]);
});

task('test', 'build', function () {
	var testDlls = fs.createScanner('src').include('*.Tests/bin/Debug/*.Tests.dll').scan();
	dotnet.runNUnit(testDlls);
});

task('release', 'test', function () {
	fs.deletePath('build');
	dotnet.deployToNuGet('src/ADOUtils/ADOUtils.csproj', 'src/ADOUtils/bin/Debug', true);
/*
	dotnet.runMSBuild('src/ExtDirectHandler.sln', [ 'Clean', 'ExtDirectHandler:Rebuild' ]);
	fs.zipPath('build/bin', 'build/extdirecthandler-' + [ version.major, version.minor, version.build, version.revision ].join('.') + '.zip');
*/
	version.build += 1;
	fs.writeFile('version.json', JSON.stringify(version));
});

