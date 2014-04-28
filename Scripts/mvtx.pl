#! perl -w
use strict;
use warnings;
use 5.010;
use Cwd qw/getcwd/;

my $MOVE_DTI = 1;
my $RMDIR_DTI = 1;
my $PLIST_THIS = 1;

sub endsWith
{
	my ($s, $sub) = @_;
	my $i = rindex($s, $sub, length($s) - 1);
	return $i >= 0 && $i + length($sub) == length($s);
}

sub matchesDelete{
	my $a = $_[0];
	return endsWith($a,'stand') || endsWith($a,'attack') ||
		endsWith($a,'hurt') || endsWith($a, 'dead') ||
		endsWith($a,'skill01') || endsWith($a,'skill02') ||
		endsWith($a,'skill')  || endsWith($a,'skill03') || endsWith($a,'skill05');
}

sub trimLastDir{
	my $ff = shift;
	my $r = rindex($ff,'/', length($ff)-1);
	return '' if $r < 0;
	my $l = rindex($ff, '/', $r-1);
	return '' if $l >= $r or $l < 0;
	return substr($ff, $l+1, $r - $l - 1);
}

sub trimPreLastDir{
	my $ff = shift;
	my $r = rindex($ff, '/', length($ff)-1);
	return '%%' if $r < 0;
	$r = rindex($ff,'/', $r-1);
	return '@@' if $r < 0;
	return substr($ff, 0, $r) . '/';
}

sub walkNow
{
	my $cd = shift or die "no current directory";
	my @arr;
	opendir my $d, $cd or die "cannot open current dir";
	while( my $f = readdir $d )
	{
		my $ff = $cd . '/' . $f;
		next if not -d $ff or $f eq '.' or $f eq '..';
		push @arr, [$ff, $f];
		&walkNow($ff);
	}
	closedir $d;

	opendir my $e, $cd or die "cannot open current dir";
	while( my $f = readdir $e )
	{
		my $ff = $cd . '/' . $f;
		next if not -f $ff or $ff !~ /dti\.txt$/imxs;

		#
		my $pre = trimPreLastDir($ff) . trimLastDir($ff) . '_pi.txt';
		my $cmd = "move \"$ff\" \"$pre\"";
		$cmd =~ s/\//\\/g;

		if(defined($MOVE_DTI))
		{
			#print $cmd,"\n";
			system($cmd);
			die if $?;
		}
	}
	closedir $e;

	# Do it !
	if(defined($PLIST_THIS))
	{
		foreach(@arr)
		{
			my ($ff, $f) = @{$_};
			if( matchesDelete($f))
			{
				my $cmd = "texturepacker --data \"$cd/$f.plist\" --sheet \"$cd/$f.pvr.ccz\" \"$ff\"";
				# print $cmd,"\n";
				system($cmd);
				die if $?;
			}
		}
	}

	# Stay in this order
	if(defined($RMDIR_DTI))
	{
		foreach(@arr)
		{
			my ($ff, $f) = @{$_};
			if( matchesDelete($f))
			{
				my $cmd = "rmdir /q/s \"$ff\"";
				print $cmd,"\n";
				system($cmd);
				die if $?;
			}
		}
	}

}

walkNow getcwd;
#print endsWith('E:/execute/EE/ours/5/4/dead', 'dead');


