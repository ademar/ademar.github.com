--- 
mt_id: 28
layout: post
title: Notes on installing Movable Type version 5.11 on Microsoft Internet Information Services (IIS)
date: 2011-07-15 11:29:24 -04:00
tags:
- cgi
- iis
- iis7
- movabletype
- perl
---
Recently migrated my blog to a new server Windows Server 2008 R2 (also changed the painting) and had to re-install Movable Type. Whenever I have done this I usually hit the following two issues:

<b>First Issue:</b>

```
HTTP Error 502.2 - Bad Gateway
The specified CGI application misbehaved by not returning a complete set of HTTP headers. The headers it did return are "Can't locate MT/Bootstrap.pm in @INC (@INC contains: lib D:/Perl/site/lib D:/Perl/lib .) at \\nas-005\winspace\10-ademar.name\www\mt\mt.cgi line 11. BEGIN failed--compilation aborted at \\nas-005\winspace\10-ademar.name\www\mt\mt.cgi line 11. ".
```

<div style="margin: 0pt 0pt 20px 20px; float: right;" ><img alt="movabletypedirectorylayout.png" src="http://ademar.name/blog/movabletypedirectorylayout.png" /> <br/> <center>Movable Type directory layout</center></div>

p. To solve this you need to convert the folder containing the Movable Type installation into a virtual directory.
Lets say you are installing it in folder mt, then you need to create a virtual directory with the same name pointing to the folder in question.

<b>Second Issue</b>

p. Plugins fail to  load. In my case Movable Type had problems loading plugins like MarkDown,SmartyPants and Textile. In the logs messages like the following could be found:

bq. 
Plugin error: \\nas-005\winspace\10-ademar.name\www\mt\plugins\Markdown\SmartyPants.pl Can't locate \nas-005\winspace\10-ademar.name\www\mt\plugins\Markdown\SmartyPants.pl in @INC (@INC contains: \\nas-005\winspace\10-ademar.name\www\mt\extlib \\nas-005\winspace\10-ademar.name\www\mt\extlib lib D:/Perl/site/lib D:/Perl/lib .) at lib/MT.pm line 1396

p.  To solve this I modified MT.pm by substituting lines 1396,1397 (as of version 5.11)

```php
eval "# line " . __LINE__ . " " . __FILE__
        . "\nrequire '$plugin';";

```
by the following:

```php
eval { require $plugin };

```

p. I'm not sure how wise is that change but it solves the problem. 
