#! perl -w
use strict;
use warnings;
use 5.010;
use Cwd qw/getcwd/;

#Final version

sub countFiles{
	my $cd = shift or die "no no no";
	my $name = shift or die;
	my $max = 0;

	opendir my $d, $cd or die "no current";
	while(my $f = readdir $d)
	{
		my $ff = $cd . '/' . $f;
		next if -d $ff;

		if( $ff =~ /(\d+)\.png/imxs)
		{
			my $v = int($1);
			if($max < $v )
			{
				$max = $v;
			}
		}
	}
	closedir $d;

	open my $fout, ">", $name or die;
	print {$fout} $max+1, ",0.1,0";
	close $fout;
	print "$name is done\n";
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
		countFiles $ff, $cd.'/'.$f.'.txt';
	}
	closedir $d;
	
	# Do it !
	foreach(@arr)
	{
		my ($ff, $f) = @{$_};
		my $cmd = "texturepacker --data \"$cd/$f.plist\" --sheet \"$cd/$f.pvr.ccz\" \"$ff\"";
		# print $cmd,"\n";
		system($cmd);
		die if $?;
	}

	# Stay in this order
	foreach(@arr)
	{
		my ($ff, $f) = @{$_};
		my $cmd = "rmdir /q/s \"$ff\"";
		print $cmd,"\n";
		system($cmd);
		die if $?;
	}
}

walkNow getcwd;
#print endsWith('E:/execute/EE/ours/5/4/dead', 'dead');


