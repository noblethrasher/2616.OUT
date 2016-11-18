Imports System.Linq
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Web
Imports ArcRx
Imports <xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

Public NotInheritable Class Represent
    Inherits Html.Representation

    ReadOnly hello_scooby As HelloScooby

    Public Sub New(hello_scooby As HelloScooby)

        Me.hello_scooby = hello_scooby

    End Sub

    Protected Overrides Sub _ProcessRequest(context As HttpContext)

        context.Response.ContentType = ContentType

        Dim xml = <Person>
                      <ID><%= hello_scooby.id %></ID>
                      <Name><%= hello_scooby.scooby %></Name>
                  </Person>.CreateReader()

        Dim xsl = <?xml version="1.0" encoding="utf-8"?>
                  <xsl:stylesheet version="1.0">
                      <xsl:output method="html" encoding="utf-8" indent="yes"/>

                      <xsl:template match="*">
                          <xsl:copy/>
                          <xsl:apply-templates/>
                      </xsl:template>

                      <xsl:template match="/">
                          <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html&gt;&#10;</xsl:text>

                          <html>
                              <xsl:text disable-output-escaping='yes'>&#10;</xsl:text>
                              <script type="text/javascript">
                                  <xsl:text>
                                        if(2 &gt; 1)
                                            alert('Hello, world');
                                    </xsl:text>
                              </script>
                              <xsl:text disable-output-escaping='yes'>&#10;</xsl:text>
                              <body>
                                  <xsl:apply-templates/>
                              </body>
                          </html>
                      </xsl:template>

                      <xsl:template match="Person">
                          <h1>
                              <xsl:attribute name="id">
                                  <xsl:value-of select="./ID"/>
                              </xsl:attribute>
                              <xsl:value-of select="./Name"/>
                          </h1>
                          <p>
                              <%= String.Join(";", context.Request.AcceptTypes.Select(Function(x) x)) %>
                          </p>
                      </xsl:template>

                  </xsl:stylesheet>.CreateReader()


        Dim xslt = New XslCompiledTransform()

        xslt.Load(xsl)

        xslt.Transform(xml, Nothing, context.Response.Output)

    End Sub

End Class

Public Class HelloScooby
    Inherits ArcRx.UrlAppRoute.AppState
    Implements [Get].Allowed

    Shared memo As New Dictionary(Of Integer, String) From
    {
        {1, "Buffy Summers"},
        {2, "Willow  Rosenberg"},
        {3, "Dawn Summers"},
        {4, "Xander Harris"}
    }

    Friend ReadOnly scooby As String
    Friend ReadOnly id As Integer

    Public Sub New(id As Integer)

        memo.TryGetValue(id, scooby)
        Me.id = id

    End Sub


    Protected Overrides Function GetRepresentation(ctx As HttpContextEx) As Representation

        Return New Represent(Me)

    End Function

    Public Function Accept(method As [Get], ctx As HttpContextEx) As Representation Implements [Get].Allowed.Accept


        Return GetRepresentation(ctx)

    End Function



End Class
