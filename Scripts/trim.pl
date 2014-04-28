#! perl -w
use strict;
use warnings;
use 5.010;
use Cwd qw/getcwd/;

my $exe = 'pngclippercmdline.exe';

sub config{
	my $cd = getcwd;
	$exe = "$cd\\$exe";
	die "no tool for $exe" if not -e $exe;
}

sub walkNow{
	my $cd = shift or die "no current directory";
	my $cmd = "$exe \"$cd\"";
	# print $cmd,"\n";
	system($cmd);

	my @nds;
	opendir my $d, $cd or die "cannot open current dir";
	while( my $f = readdir $d )
	{
		my $ff = $cd . '\\' . $f;
		next if not -d $ff or $f eq '.' or $f eq '..';
		push @nds, $ff;
	}
	closedir $d;

	walkNow($_) for(@nds);
}

sub main{
	config;
	my $now = getcwd;
	walkNow $now;
}

main;
