<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/">
    <html lang="uk">
      <head>
        <meta charset="utf-8" />
        <title>Інформаційні ресурси кафедри</title>
        <style>
          body {
            font-family: "Segoe UI", Arial, sans-serif;
            font-size: 14px;
            line-height: 1.45;
            color: #222222;
            background-color: #ffffff;
            margin: 24px;
          }
          h1 {
            font-size: 20px;
            font-weight: 600;
            color: #1f2937;
            margin: 0 0 6px 0;
          }
          .meta {
            font-size: 13px;
            color: #6b7280;
            margin-bottom: 18px;
          }
          table {
            width: 100%;
            border-collapse: collapse;
            border: 1px solid #d1d5db;
          }
          th {
            background-color: #f3f4f6;
            color: #111827;
            font-weight: 600;
            text-align: left;
            padding: 9px 10px;
            border: 1px solid #d1d5db;
          }
          td {
            padding: 8px 10px;
            border: 1px solid #e5e7eb;
            vertical-align: top;
          }
          tr:hover td {
            background-color: #f9fafb;
          }
          .num {
            width: 32px;
            text-align: center;
            color: #6b7280;
          }
          .title {
            font-weight: 600;
            color: #111827;
          }
          a {
            color: #374151;
            text-decoration: underline;
            word-break: break-all;
          }
          a:hover {
            color: #111827;
          }
        </style>
      </head>
      <body>
        <h1>Інформаційні ресурси кафедри</h1>
        <div class="meta">
          Усього ресурсів: <xsl:value-of select="count(/DepartmentInformationResources/*/*)" />
        </div>

        <table>
          <thead>
            <tr>
              <th class="num">№</th>
              <th>Назва ресурсу</th>
              <th>Вид</th>
              <th>Анотація</th>
              <th>П.І.П. автора</th>
              <th>Факультет</th>
              <th>Кафедра</th>
              <th>Умови використання</th>
              <th>Адреса</th>
            </tr>
          </thead>
          <tbody>
            <xsl:for-each select="/DepartmentInformationResources/*/*">
              <tr>
                <td class="num"><xsl:value-of select="position()" /></td>
                <td class="title"><xsl:value-of select="Title" /></td>
                <td>
                  <xsl:choose>
                    <xsl:when test="@type"><xsl:value-of select="@type" /></xsl:when>
                    <xsl:when test="@Type"><xsl:value-of select="@Type" /></xsl:when>
                    <xsl:otherwise><xsl:value-of select="name()" /></xsl:otherwise>
                  </xsl:choose>
                </td>
                <td><xsl:value-of select="Annotation" /></td>
                <td><xsl:value-of select="Author" /></td>
                <td>
                  <xsl:choose>
                    <xsl:when test="@faculty"><xsl:value-of select="@faculty" /></xsl:when>
                    <xsl:when test="../@faculty"><xsl:value-of select="../@faculty" /></xsl:when>
                    <xsl:otherwise><xsl:value-of select="Faculty" /></xsl:otherwise>
                  </xsl:choose>
                </td>
                <td>
                  <xsl:choose>
                    <xsl:when test="@department"><xsl:value-of select="@department" /></xsl:when>
                    <xsl:when test="../@department"><xsl:value-of select="../@department" /></xsl:when>
                    <xsl:otherwise><xsl:value-of select="Department" /></xsl:otherwise>
                  </xsl:choose>
                </td>
                <td><xsl:value-of select="UsageConditions" /></td>
                <td>
                  <xsl:if test="Address != ''">
                    <a href="{Address}" target="_blank"><xsl:value-of select="Address" /></a>
                  </xsl:if>
                </td>
              </tr>
            </xsl:for-each>
          </tbody>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>