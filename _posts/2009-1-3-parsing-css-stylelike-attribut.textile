--- 
mt_id: 13
layout: post
title: Parsing CSS style-like attributes with XSLT
date: 2009-01-03 10:31:38 -05:00
tags:
- css
- xml
- xslt
---
p.. Lets say we need to transform @<div style="width:236px; height:33px" />@ into @<element width="236" height="33" />@

The following snippet does it with the help of the function @getCssProperty@:

bc.. 
<xsl:template match="div" >
    <xsl:variable name="height" 
        select="substring-before(melon:getCssProperty(@style,'height'),'px')"/>
    <xsl:variable name="width" 
        select="substring-before(melon:getCssProperty(@style,'width'),'px')"/>
    <element width="{$width}" height="{$height}" />
</xsl:template>


p.. 
getCssProperty definition:

bc.. 
<xsl:function name="melon:getCssProperty">
    <xsl:param name="style"/>
    <xsl:param name="propertyName"/>
    <xsl:for-each select="tokenize($style,';')">
        <xsl:if test="normalize-space(substring-before(.,':'))=$propertyName">
            <xsl:value-of select="normalize-space(substring-after(.,':'))" />
        </xsl:if>
    </xsl:for-each>
</xsl:function>

p.. 
<b>Update:</b> I noticed XSLT 1.0 does not support the construct @<xsl:function>@, so I end up using the following expression:

bc.. 
<xsl:variable name="height" 
    select="normalize-space(substring-before(substring-after(@style,'height:'),'px'))"/>
<xsl:variable name="width" 
    select="normalize-space(substring-before(substring-after(@style,'width:'),'px'))"/> 
