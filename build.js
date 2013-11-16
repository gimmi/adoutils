load('jsmake.dotnet.DotNetUtils.js');

var fs = jsmake.Fs;
var utils = jsmake.Utils;
var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

var version, assemblyVersion;

task('default', 'build');

task('version', function () {
	version = JSON.parse(fs.readFile('version.json'));
	assemblyVersion = [ version.major, version.minor, version.build, 0 ].join('.');
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
		AssemblyCopyright: 'Copyright � Gian Marco Gherardi ' + new Date().getFullYear(),
		AssemblyTrademark: '',
		AssemblyCompany: 'Gian Marco Gherardi',
		AssemblyConfiguration: '', // Probably a good place to put Git SHA1 and build date
		AssemblyVersion: assemblyVersion,
		AssemblyFileVersion: assemblyVersion,
		AssemblyInformationalVersion: assemblyVersion
	});
});

task('build', [ 'dependencies', 'assemblyinfo' ], function () {
	dotnet.runMSBuild('src/ADOUtils.sln', [ 'Clean', 'Rebuild' ]);
});

task('test', 'build', function () {
	jsmake.Sys.createRunner(dotnet._nugetPath)
		.args('install', 'NUnit.Runners')
		.args('-Version', '2.6.3')
		.args('-OutputDirectory', 'tools')
		.run();

	dotnet._nunitPath = 'tools/NUnit.Runners.2.6.3/tools/nunit-console.exe';

	var testDlls = fs.createScanner('src').include('*.Tests/bin/Debug/*.Tests.dll').scan();
	dotnet.runNUnit(testDlls);
});

task('release', 'test', function () {
	fs.deletePath('build');
	fs.createDirectory('build');

	sys.run('tools/nuget/nuget.exe', 'pack', 'src\\ADOUtils\\ADOUtils.csproj', '-Build', '-OutputDirectory', 'build', '-Symbols');
	sys.run('tools/nuget/nuget.exe', 'push', 'build\\ADOUtils.' + assemblyVersion + '.nupkg');

	version.build += 1;
	fs.writeFile('version.json', JSON.stringify(version));
});

