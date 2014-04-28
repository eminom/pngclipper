#! perl -w

use 5.010;
use strict;
use warnings;
use Cwd qw/getcwd/;

sub walkNow
{
	my $cd = shift or die "no current directory";
	my @arr;
	opendir my $d, $cd or die "cannot open current dir";
	while( my $f = readdir $d )
	{
		my $ff = $cd . '/' . $f;
		next if not -d $ff or $f eq '.' or $f eq '..';
		push @arr, $ff;
		&walkNow($ff);
	}
	closedir $d;

	my @sa;
	opendir my $e, $cd or die "cannot open current dir";
	while( my $f = readdir $e )
	{
		my $ff = $cd . '/' . $f;
		next if not -f $ff or $f !~ /(\d+)\.png$/imxs;
		my $v = 0 + $1;
		push @sa, [$f, $v];
	}
	closedir $e;

	@sa = sort {$a->[1] <=> $b->[1]} @sa;
	my $a = 0;
	my @nar;
	push @nar, [$cd, $_->[0], sprintf("%d.png", $a++)] for @sa;

	# Take over
	@sa = @nar;

	# Mapping done
	for(@sa)
	{
		my ($cd, $f, $n) = @{$_};
		my $cmd = "move \"$cd\\$f\" \"$cd\\$n\"";
		$cmd =~ s/\//\\/g;
		print $cmd,"\n";
		system($cmd);
		die if $?;
	}
}

walkNow getcwd;




