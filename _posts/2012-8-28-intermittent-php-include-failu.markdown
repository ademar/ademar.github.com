--- 
mt_id: 29
layout: post
title: Intermittent PHP include failures on Microsoft IIS
date: 2012-08-28 09:40:01 -04:00
tags:
- fastcgi
- iis
- iis7
- php
- wordpress
---
This is an answer to my own [StackOverflow question](http://superuser.com/questions/285869/php-include-and-require-statements-fails-after-a-number-of-executions) that I can not answer over there because of lacking enough reputation.

This issue was due to a missing permission. What I eventually found out is that PHP (or one of its modules like php_wincache.dll) wants Execute/Traverse permissions in the website's root directory.

With the help of SysInternals Process Monitor I noticed that the following CreateFile call was failing with Access Denied. Note in the desired access field is asking for Execute/Traverse

![CreateFile returns access denied][1]

After that, PHP tries to load the include file from the wrong location.

![enter image description here][2]

Solution: granting Execute/Traverse in the website's root directory to the identity under which the PHP process is running solve the intermittent 'No such file or directory' include issue.

  [1]: http://i.stack.imgur.com/ff0zE.png
  [2]: http://i.stack.imgur.com/cOd8M.png 
