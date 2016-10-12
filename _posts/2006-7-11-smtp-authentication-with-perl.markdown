--- 
mt_id: 5
layout: post
title: SMTP authentication with Perl
date: 2006-07-11 10:44:58 -04:00
tags:
- authentication
- perl
- smtp
---

```perl 

#!/usr/local/bin/perl -w
use Net::SMTP;
$recipient = 'somebody@somewhere.com' ;
$from	   = 'not.me@ademar.name' ;
$smtp = Net::SMTP->new('smtp.ademar.name', 
			Timeout => 60,
			Debug   => 1);
## smtp server requires authentication 
$smtp->auth('not.me@ademar.name','*******');
$smtp->mail($from);
$smtp->to($recipient);

$smtp->data();
$smtp->datasend("To: $recipient\n");
$smtp->datasend("Subject: test message\n");
$smtp->datasend("\n");
$smtp->datasend("A simple test message\n");
$smtp->dataend();

$smtp->quit;

``` 

Something funny, while testing the above script, my Postfix server complained with the following message -

<em>221 Error: I can break rules, too. Goodbye.</em> - i LOLed.

 
