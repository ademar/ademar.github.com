--- 
mt_id: 6
layout: post
title: "Curl: SSL certificate problem, verify that the CA cert is OK"
date: 2006-04-07 11:24:34 -04:00
tags:
- certificates
- curl
- error
- man-in-the-middle
- php
- security
- ssl
---
When opening a secure url with Curl you may get the following error:

<em>SSL certificate problem, verify that the CA cert is OK</em>

I will explain why the error and what you should do about it.

The easiest way of getting rid of the error would be adding the following two lines to your script . This solution poses a security risk tho.

<pre>
<code>
//WARNING: this would prevent curl from detecting a 'man in the middle' attack
curl_setopt ($ch, CURLOPT_SSL_VERIFYHOST, 0);
curl_setopt ($ch, CURLOPT_SSL_VERIFYPEER, 0); 
</code>
</pre>

p.. 
Let see what this two parameters do. Quoting the manual.<br/>
<blockquote>
    CURLOPT_SSL_VERIFYHOST: 1 to check the existence of a common name in the SSL peer certificate. 2 to check the existence of a common name and also verify that it matches the hostname provided.<br/>
    CURLOPT_SSL_VERIFYPEER: FALSE to stop CURL from verifying the peer's certificate. Alternate certificates to verify against can be specified with the CURLOPT_CAINFO option or a certificate directory can be specified with the CURLOPT_CAPATH option. CURLOPT_SSL_VERIFYHOST may also need to be TRUE or FALSE if CURLOPT_SSL_VERIFYPEER is disabled (it defaults to 2). 
</blockquote>

Setting CURLOPT_SSL_VERIFYHOST to 2 (This is the default value) will garantee that the certificate being presented to you have a 'common name' matching the URN you are using to access the remote resource. This is a healthy check but it doesn't guarantee your program is not being decieved.

<b>Enter the 'man in the middle'</b>

Your program could be misleaded into talking to another server instead. This can be achieved through several mechanisms, like dns or arp poisoning ( This is a story for another day). The intruder can also self-sign a certificate with the same 'comon name' your program is expecting. The communication would still be encrypted but you would be giving away your secrets to an impostor. This kind of attack is called 'man in the middle'

<b>Defeating the 'man in the middle'</b>

Well, we need to to verify the certificate being presented to us is good for real. We do this by comparing it against a certificate we reasonable* trust.

If the remote resource is protected by a certificate issued by one of the main CA's like Verisign, GeoTrust et al, you can safely compare against Mozilla's CA certificate bundle which you can get from <a href="http://curl.haxx.se/docs/caextract.html">http://curl.haxx.se/docs/caextract.html</a>

Save the file cacert.pem somewhere in your server and set the following options in your script.

<pre>
<code> 
curl_setopt ($ch, CURLOPT_SSL_VERIFYPEER, TRUE); 
curl_setopt ($ch, CURLOPT_CAINFO, "pathto/cacert.pem");
</code>
</pre>

p.. 
If you are connecting to a resource protected by a self-signed certificate, all you need to do is obtain a copy of the certificate in PEM format and append it to the cacert.pem of the above paragraph.

By now you should be pretty safe from the man in the middle. But there is still a weak point here, there is nothing that says the trust chain can't been subverted. In fact it have happened before : <a href="http://news.cnet.com/2100-1001-254586.html">http://news.cnet.com/2100-1001-254586.html</a> 
