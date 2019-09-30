<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
                exclude-result-prefixes="edm edm2 edm3 edmx1 edmx2 edmx3 m annotation"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:edm="http://docs.oasis-open.org/odata/ns/edm"
                xmlns:edm2="http://schemas.microsoft.com/ado/2008/09/edm"
                xmlns:edm3="http://schemas.microsoft.com/ado/2009/11/edm"
                xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx" 
                xmlns:edmx1="http://schemas.microsoft.com/ado/2007/06/edmx"
                xmlns:edmx2="http://schemas.microsoft.com/ado/2008/09/edmx"
                xmlns:edmx3="http://schemas.microsoft.com/ado/2009/11/edmx"
                xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"
                xmlns:annotation="http://schemas.microsoft.com/ado/2009/02/edm/annotation"
                xmlns="http://docs.oasis-open.org/odata/ns/edm"
>
  <!--
    This style sheet transforms OData 2.0 or 3.0 $metadata documents into OData 4.0 CSDL documents.
    Existing constructs that have an equivalent in V4 are automatically translated.
    The retired primitive type Edm.DateTime is translated into Edm.DateTimeOffset or Edm.Date.
    The retired primitive type Edm.Time is translated into Edm.TimeOfDay.
    Modified from: https://github.com/oasis-tcs/odata-openapi/blob/82d4ba6cfb20ec8f81fc04ea61a36423dab88ed0/tools/V2-to-V4-CSDL.xsl
  -->
  <xsl:strip-space elements="*" />
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" encoding="utf-8" />

  <xsl:variable name="Core">
    <xsl:call-template name="include-alias">
      <xsl:with-param name="schema" select="'Org.OData.Core.V1'" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="Capabilities">
    <xsl:call-template name="include-alias">
      <xsl:with-param name="schema" select="'Org.OData.Capabilities.V1'" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="Measures">
    <xsl:call-template name="include-alias">
      <xsl:with-param name="schema" select="'Org.OData.Measures.V1'" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="Aggregation">
    <xsl:call-template name="include-alias">
      <xsl:with-param name="schema" select="'Org.OData.Aggregation.V1'" />
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="Validation">
    <xsl:call-template name="include-alias">
      <xsl:with-param name="schema" select="'Org.OData.Validation.V1'" />
    </xsl:call-template>
  </xsl:variable>

  <xsl:template match="edmx1:Edmx|edmx2:Edmx|edmx3:Edmx|edmx:Edmx">
    <edmx:Edmx Version="4.0">
      <xsl:call-template name="add-reference">
        <xsl:with-param name="condition" select="//edm2:Summary|//edm2:LongDescription|//edm3:Summary|//edm3:LongDescription" />
        <xsl:with-param name="schema" select="'Org.OData.Core.V1'" />
      </xsl:call-template>
      <xsl:apply-templates />
    </edmx:Edmx>
  </xsl:template>

  <xsl:template name="include-alias">
    <xsl:param name="schema" />
    <xsl:choose>
      <xsl:when test="//edmx:Include[@Namespace=$schema]/@Alias">
        <xsl:value-of select="//edmx:Include[@Namespace=$schema]/@Alias" />
      </xsl:when>
      <xsl:when test="//edmx:Include[@Namespace=$schema]">
        <xsl:value-of select="$schema" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="default-alias">
          <xsl:with-param name="schema" select="$schema" />
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="default-alias">
    <xsl:param name="schema" />
    <xsl:call-template name="substring-after-last">
      <xsl:with-param name="input">
        <xsl:call-template name="substring-before-last">
          <xsl:with-param name="input" select="$schema" />
          <xsl:with-param name="marker" select="'.'" />
        </xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="marker" select="'.'" />
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="add-reference">
    <xsl:param name="condition" />
    <xsl:param name="schema" />
    <xsl:param name="wikipage" select="false()" />
    <xsl:if test="$condition">
      <xsl:if test="not(//edmx:Include[@Namespace=$schema])">
        <xsl:variable name="alias">
          <xsl:call-template name="default-alias">
            <xsl:with-param name="schema" select="$schema" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="uri">
          <xsl:choose>
            <xsl:when test="substring($schema,1,10)='Org.OData.'">
              <xsl:text>https://oasis-tcs.github.io/odata-vocabularies/vocabularies/</xsl:text>
              <xsl:value-of select="$schema" />
              <xsl:text>.xml</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:variable>
        <edmx:Reference>
          <xsl:attribute name="Uri">
            <xsl:value-of select="$uri" />
          </xsl:attribute>
          <edmx:Include>
            <xsl:attribute name="Namespace">
              <xsl:value-of select="$schema" />
            </xsl:attribute>
            <xsl:attribute name="Alias">
              <xsl:value-of select="$alias" />
            </xsl:attribute>
          </edmx:Include>
        </edmx:Reference>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="edmx:Reference">
    <edmx:Reference>
      <xsl:apply-templates select="@*|node()" />
    </edmx:Reference>
  </xsl:template>

  <xsl:template match="edmx:Include">
    <edmx:Include>
      <xsl:copy-of select="@Namespace|@Alias" />
    </edmx:Include>
  </xsl:template>

  <xsl:template match="edmx:IncludeAnnotations">
    <edmx:IncludeAnnotations>
      <xsl:copy-of select="@Namespace|@Qualifier|@TargetNamespace" />
    </edmx:IncludeAnnotations>
  </xsl:template>

  <xsl:template match="edmx1:DataServices|edmx2:DataServices|edmx3:DataServices|edmx:DataServices">
    <edmx:DataServices>
      <xsl:apply-templates />
    </edmx:DataServices>
  </xsl:template>

  <xsl:template match="edm2:Schema">
    <Schema xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <xsl:copy-of select="@Namespace|@Alias" />
      <xsl:apply-templates />
      <xsl:apply-templates select="*[local-name()='EntityContainer' and @m:IsDefaultEntityContainer='true']/*[local-name()='FunctionImport']" mode="Schema" />
    </Schema>
  </xsl:template>

  <xsl:template match="edm3:Schema">
    <Schema xmlns="http://docs.oasis-open.org/odata/ns/edm">
      <xsl:copy-of select="@Namespace|@Alias" />
      <xsl:apply-templates />
      <xsl:apply-templates select="*[local-name()='EntityContainer' and @m:IsDefaultEntityContainer='true']/*[local-name()='FunctionImport']" mode="Schema" />
    </Schema>
  </xsl:template>

  <xsl:template match="edm2:EntityContainer|edm3:EntityContainer">
    <xsl:if test="@m:IsDefaultEntityContainer='true'">
      <EntityContainer>
        <xsl:apply-templates select="@*|node()" />
      </EntityContainer>
    </xsl:if>
  </xsl:template>

  <xsl:template match="edm2:Property|edm3:Property">
    <Property>
      <xsl:copy-of select="@Name" />
      <xsl:apply-templates select="@Type" />
      <xsl:if test="@Nullable != 'true'">
        <xsl:copy-of select="@Nullable" />
      </xsl:if>
      <xsl:apply-templates select="@DefaultValue|@MaxLength|@Precision|@Scale|@Unicode|@SRID" />
    </Property>
  </xsl:template>

  <xsl:template match="@MaxLength[.='Max']">
    <xsl:attribute name="MaxLength">max</xsl:attribute>
  </xsl:template>

  <xsl:template match="edm2:NavigationProperty|edm3:NavigationProperty">
    <NavigationProperty>
      <xsl:copy-of select="@Name" />
      <!-- Extract @Type and @Multiplicity from matching Association/End -->
      <xsl:variable name="relation" select="@Relationship" />
      <xsl:variable name="assoc">
        <xsl:call-template name="substring-after-last">
          <xsl:with-param name="input" select="@Relationship" />
          <xsl:with-param name="marker" select="'.'" />
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="fromrole" select="@FromRole" />
      <xsl:variable name="torole" select="@ToRole" />
      <xsl:variable name="type" select="../../..//edm2:Association[@Name=$assoc]/edm2:End[@Role=$torole]/@Type|../../..//edm3:Association[@Name=$assoc]/edm3:End[@Role=$torole]/@Type" />
      <xsl:variable name="mult" select="../../..//edm2:Association[@Name=$assoc]/edm2:End[@Role=$torole]/@Multiplicity|../../..//edm3:Association[@Name=$assoc]/edm3:End[@Role=$torole]/@Multiplicity" />
      <xsl:attribute name="Type">
        <xsl:choose>
          <xsl:when test="$mult='*'">
            <xsl:value-of select="concat('Collection(',$type,')')" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$type" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:if test="$mult='1'">
        <xsl:attribute name="Nullable">false</xsl:attribute>
      </xsl:if>
      <xsl:variable name="partner" select="../../..//edm2:EntityType/edm2:NavigationProperty[@Relationship=$relation and @FromRole=$torole]/@Name|../../..//edm3:EntityType/edm3:NavigationProperty[@Relationship=$relation and @FromRole=$torole]/@Name" />
      <xsl:if test="$partner">
        <xsl:attribute name="Partner">
          <xsl:value-of select="$partner" />
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates mode="NavProp" select="../../..//edm2:Association[@Name=$assoc]/edm2:End[@Role=$fromrole]/edm2:OnDelete|../../..//edm3:Association[@Name=$assoc]/edm3:End[@Role=$fromrole]/edm3:OnDelete" />
      <xsl:apply-templates mode="NavProp" select="../../..//edm2:Association[@Name=$assoc]/edm2:ReferentialConstraint/edm2:Principal[@Role=$torole]|../../..//edm3:Association[@Name=$assoc]/edm3:ReferentialConstraint/edm3:Principal[@Role=$torole]" />
      <xsl:apply-templates />
    </NavigationProperty>
  </xsl:template>

  <xsl:template match="edm2:OnDelete|edm3:OnDelete" mode="NavProp">
    <OnDelete>
      <xsl:copy-of select="@Action" />
      <xsl:apply-templates />
    </OnDelete>
  </xsl:template>

  <xsl:template match="edm2:PropertyRef|edm3:PropertyRef" mode="NavProp">
    <xsl:variable name="index" select="position()" />
    <ReferentialConstraint>
      <xsl:attribute name="Property">
        <xsl:value-of
        select="../../..//edm2:Dependent/edm2:PropertyRef[$index]/@Name|../../..//edm3:Dependent/edm2:PropertyRef[$index]/@Name" />
      </xsl:attribute>
      <xsl:attribute name="ReferencedProperty">
        <xsl:value-of select="@Name" />
      </xsl:attribute>
    </ReferentialConstraint>
  </xsl:template>

  <xsl:template match="edm2:EntitySet|edm3:EntitySet">
    <xsl:variable name="qualifier">
      <xsl:call-template name="substring-before-last">
        <xsl:with-param name="input" select="@EntityType" />
        <xsl:with-param name="marker" select="'.'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="namespace">
      <xsl:choose>
        <xsl:when test="//edm:Schema[@Alias=$qualifier]">
          <xsl:value-of select="//edm:Schema[@Alias=$qualifier]/@Namespace" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$qualifier" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="type">
      <xsl:call-template name="substring-after-last">
        <xsl:with-param name="input" select="@EntityType" />
        <xsl:with-param name="marker" select="'.'" />
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="name" select="@Name" />

    <EntitySet>
      <xsl:copy-of select="@Name|@EntityType" />
      <xsl:apply-templates select="../edm2:AssociationSet/edm2:End[@EntitySet=$name]|../edm3:AssociationSet/edm3:End[@EntitySet=$name]" mode="Binding">
        <xsl:with-param name="entitytype" select="@EntityType" />
      </xsl:apply-templates>
    </EntitySet>
  </xsl:template>

  <xsl:template match="edm2:AssociationSet/edm2:End|edm3:AssociationSet/edm3:End" mode="Binding">
    <xsl:param name="entitytype" />
    <xsl:variable name="role" select="@Role" />
    <xsl:variable name="set" select="../edm2:End[not(@Role=$role)]/@EntitySet|../edm3:End[not(@Role=$role)]/@EntitySet" />
    <xsl:variable name="assoc" select="../@Association" />
    <xsl:variable name="navprop" select="//edm2:NavigationProperty[@Relationship=$assoc and @FromRole=$role]|//edm3:NavigationProperty[@Relationship=$assoc and @FromRole=$role]" />
    <xsl:if test="$navprop">
      <xsl:variable name="namespace" select="$navprop/../../..//@Namespace" />
      <xsl:variable name="typename" select="$navprop/../@Name" />
      <xsl:variable name="type" select="concat($namespace,'.',$typename)" />
      <NavigationPropertyBinding>
        <xsl:attribute name="Target">
          <xsl:value-of select="$set" />
        </xsl:attribute>
        <xsl:attribute name="Path">
          <xsl:if test="not($type=$entitytype)">
            <xsl:value-of select="concat($type,'/')" />
          </xsl:if>
          <xsl:value-of select="$navprop/@Name" />
        </xsl:attribute>
        <xsl:apply-templates />
      </NavigationPropertyBinding>
    </xsl:if>
  </xsl:template>

  <xsl:template match="edm2:FunctionImport|edm3:FunctionImport">
    <xsl:if test="not(@IsBindable='true')">
      <xsl:choose>
        <xsl:when test="@m:HttpMethod='GET' or @IsSideEffecting='false'">
          <FunctionImport>
            <xsl:copy-of select="@Name|@EntitySet" />
            <xsl:attribute name="Function">
              <xsl:value-of select="../../..//@Namespace" />.<xsl:value-of select="@Name" />
            </xsl:attribute>
            <xsl:if test="not(*[local-name()='Parameter'])">
              <xsl:attribute name="IncludeInServiceDocument">true</xsl:attribute>
            </xsl:if>
          </FunctionImport>
        </xsl:when>
        <xsl:otherwise>
          <ActionImport>
            <xsl:copy-of select="@Name|@EntitySet" />
            <xsl:attribute name="Action">
              <xsl:value-of select="../../..//@Namespace" />.<xsl:value-of select="@Name" />
            </xsl:attribute>
          </ActionImport>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <xsl:template match="edm2:FunctionImport|edm3:FunctionImport" mode="Schema">
    <xsl:choose>
      <xsl:when test="@m:HttpMethod='GET' or @IsSideEffecting='false'">
        <Function>
          <xsl:copy-of select="@Name|@EntitySetPath|@IsComposable" />
          <xsl:if test="@IsBindable">
            <xsl:attribute name="IsBound">
              <xsl:value-of select="@IsBindable" />
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
          <xsl:if test="@ReturnType">
            <ReturnType>
              <xsl:attribute name="Type">
                <xsl:value-of select="@ReturnType" />
              </xsl:attribute>
              <xsl:attribute name="Nullable">false</xsl:attribute>
            </ReturnType>
          </xsl:if>
        </Function>
      </xsl:when>
      <xsl:otherwise>
        <Action>
          <xsl:copy-of select="@Name|@EntitySetPath" />
          <xsl:if test="@IsBindable">
            <xsl:attribute name="IsBound">
              <xsl:value-of select="@IsBindable" />
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates />
          <xsl:if test="@ReturnType">
            <ReturnType>
              <xsl:attribute name="Type">
                <xsl:value-of select="@ReturnType" />
              </xsl:attribute>
              <xsl:attribute name="Nullable">false</xsl:attribute>
            </ReturnType>
          </xsl:if>
        </Action>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="edm2:Parameter|edm3:Parameter">
    <xsl:element name="{local-name()}">
      <xsl:if test="not(@Nullable)">
        <xsl:attribute name="Nullable">false</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="@*|node()" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="edm2:Function|edm3:Function">
    <Function>
      <xsl:apply-templates select="@Name|node()" />
      <xsl:if test="@ReturnType">
        <ReturnType>
          <xsl:attribute name="Type">
            <xsl:if test="not(contains(@ReturnType,'.'))">
              <xsl:text>Edm.</xsl:text>
            </xsl:if>
            <xsl:value-of select="@ReturnType" />
          </xsl:attribute>
          <xsl:apply-templates select="@*[name() != 'Name' and name() != 'ReturnType']" />
        </ReturnType>
      </xsl:if>
    </Function>
  </xsl:template>

  <xsl:template match="edm2:Documentation|edm3:Documentation">
    <!-- ignore this node and translate children -->
    <xsl:apply-templates />
  </xsl:template>

  <xsl:template match="edm2:Summary|edm3:Summary">
    <Annotation>
      <xsl:attribute name="Term">
        <xsl:value-of select="$Core" />
        <xsl:text>.Description</xsl:text>
      </xsl:attribute>
      <String>
        <xsl:value-of select="." />
      </String>
    </Annotation>
  </xsl:template>

  <xsl:template match="edm2:LongDescription|edm3:LongDescription">
    <xsl:if test=".!=''">
      <Annotation>
        <xsl:attribute name="Term">
          <xsl:value-of select="$Core" />
          <xsl:text>.LongDescription</xsl:text>
        </xsl:attribute>
        <String>
          <xsl:call-template name="replace-all">
            <xsl:with-param name="string" select="." />
            <xsl:with-param name="old" select="'&#x0A;'" />
            <xsl:with-param name="new" select="'  &#x0A;'" />
          </xsl:call-template>
        </String>
      </Annotation>
    </xsl:if>
  </xsl:template>

  <xsl:template match="edm3:ValueTerm">
    <Term>
      <xsl:apply-templates select="@*|node()" />
    </Term>
  </xsl:template>

  <xsl:template match="edm3:ValueAnnotation">
    <Annotation>
      <xsl:apply-templates select="@*|node()" />
    </Annotation>
  </xsl:template>

  <xsl:template match="edm3:TypeAnnotation">
    <Annotation>
      <xsl:attribute name="Term">
        <xsl:value-of select="@Term" />
      </xsl:attribute>
      <Record>
        <xsl:apply-templates select="node()" />
      </Record>
    </Annotation>
  </xsl:template>

  <xsl:template match="edm3:Annotations">
    <Annotations>
      <xsl:apply-templates select="@*|node()" />
    </Annotations>
  </xsl:template>

  <xsl:template match="edm3:Binary">
    <Binary>
      <xsl:comment>
        TODO: convert to base64url
      </xsl:comment>
      <xsl:apply-templates select="text()" />
    </Binary>
  </xsl:template>

  <xsl:template match="edm3:DateTime">
    <DateTimeOffset>
      <xsl:apply-templates select="text()" />
      <xsl:text>Z</xsl:text>
    </DateTimeOffset>
  </xsl:template>

  <xsl:template match="edm3:IsType">
    <IsOf>
      <xsl:apply-templates select="@*|node()" />
    </IsOf>
  </xsl:template>

  <xsl:template match="edm3:AssertType">
    <Cast>
      <xsl:apply-templates select="@*|node()" />
    </Cast>
  </xsl:template>

  <xsl:template match="@Type|@UnderlyingType">
    <xsl:attribute name="{name()}">
      <xsl:choose>
        <xsl:when test=".='Time' or .='Edm.Time'">Edm.TimeOfDay</xsl:when>
        <xsl:when test=".='Float' or .='Edm.Float'">Edm.Single</xsl:when>
        <xsl:when test=".='DateTime' or .='Edm.DateTime'">Edm.DateTimeOffset</xsl:when>
        <xsl:when test="contains(.,'.')">
          <xsl:value-of select="." />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="concat('Edm.',.)" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="@m:HasStream">
    <xsl:attribute name="HasStream">
      <xsl:value-of select="." />
    </xsl:attribute>
  </xsl:template>

  <!-- ignore -->
  <xsl:template match="edm2:Association|edm2:AssociationSet|edm2:Using" />
  <xsl:template match="edm3:Association|edm3:AssociationSet|edm3:Using" />
  <xsl:template match="@Collation|@FixedLength|@Mode|edm2:Parameter/@DefaultValue|edm3:Parameter/@DefaultValue" />
  <xsl:template match="@m:IsDefaultEntityContainer" />
  <xsl:template match="@annotation:*" />

  <!-- literally copy from V2, V3, and V4 edm namespaces -->
  <xsl:template match="edm:*|edm2:*|edm3:*">
    <xsl:element name="{local-name()}">
      <xsl:apply-templates select="@*|node()" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="@*">
    <xsl:copy />
  </xsl:template>

  <!-- get all but last segment -->
  <xsl:template name="substring-before-last">
    <xsl:param name="input" />
    <xsl:param name="marker" />
    <xsl:if test="contains($input,$marker)">
      <xsl:value-of select="substring-before($input,$marker)" />
      <xsl:if test="contains(substring-after($input,$marker),$marker)">
        <xsl:value-of select="$marker" />
        <xsl:call-template name="substring-before-last">
          <xsl:with-param name="input" select="substring-after($input,$marker)" />
          <xsl:with-param name="marker" select="$marker" />
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- get last segment -->
  <xsl:template name="substring-after-last">
    <xsl:param name="input" />
    <xsl:param name="marker" />
    <xsl:choose>
      <xsl:when test="contains($input,$marker)">
        <xsl:call-template name="substring-after-last">
          <xsl:with-param name="input" select="substring-after($input,$marker)" />
          <xsl:with-param name="marker" select="$marker" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$input" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="replace-all">
    <xsl:param name="string" />
    <xsl:param name="old" />
    <xsl:param name="new" />
    <xsl:choose>
      <xsl:when test="contains($string,$old)">
        <xsl:value-of select="substring-before($string,$old)" />
        <xsl:value-of select="$new" />
        <xsl:call-template name="replace-all">
          <xsl:with-param name="string" select="substring-after($string,$old)" />
          <xsl:with-param name="old" select="$old" />
          <xsl:with-param name="new" select="$new" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$string" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>