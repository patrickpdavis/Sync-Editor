<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">

<html>
<body>

  <h1>Requirement Specification</h1>

  <!--
  <h2>Use Case Diagram</h2>
  <img src ="use_case_diagram.gif" />
  -->

  <xsl:for-each select="/doc/assembly">
    <a>
      <xsl:attribute name="name">
        <xsl:value-of select="name" />
      </xsl:attribute>
      <h2>
        <xsl:value-of select="name" />
        V<xsl:value-of select="version" />
      </h2>
    </a>
    <table border="1" cellpadding="4" cellspacing="0">
      <tr bgcolor="#c0c0c0">
        <td><b>Requirement</b></td>
        <td><b>Type</b></td>
        <td><b>Summary</b></td>
      </tr>
      <xsl:for-each select="/doc/requirements/requirement">
        <tr>
          <td>
            <xsl:value-of select="substring-after(@name,':')" />
          </td>
          <td>
            <xsl:if test="starts-with(@name,'C:')">
              Constraint
            </xsl:if>
            <xsl:if test="starts-with(@name,'F:')">
              Functional
            </xsl:if>
            <xsl:if test="starts-with(@name,'R:')">
              <i>Risk</i>
            </xsl:if>
            <xsl:if test="starts-with(@name,'P:')">
              Performance
            </xsl:if>
          </td>
          <td>
            <xsl:value-of select="summary" />
          </td>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:for-each>

</body>
</html>

</xsl:template>
</xsl:stylesheet>
