
# $AdminURL = "https://m365x185170-admin.sharepoint.com"
# $username = "admin@M365x185170.onmicrosoft.com" 
# $clSite = "https://m365x185170.sharepoint.com/sites/CustomLearningforOffice365"
# $clSiteRel = "/sites/CustomLearningforOffice365"
#$AdminURL = "https://vnextday-admin.sharepoint.com"
#$username = "juliet@vNextDay.onmicrosoft.com" 
#$clSite = "https://vnextday.sharepoint.com/sites/CLO365_WinterUpdate"
#$clSiteRel = "/sites/CLO365_WinterUpdate"
$AdminURL = "https://sympraxis-admin.sharepoint.com"
$username = "julie.turner@sympraxisconsulting.com" 
$clSite = "https://sympraxis.sharepoint.com/sites/MicrosoftTraining"
#$clSiteRel = "/sites/MicrosoftTraining"

## WRITE CRAP TO SCREEN

$optInTelemetry = true

$userCredential = Get-Credential -UserName $username -Message "Type the password."

try {
	$conn = Connect-PnPOnline -Url $AdminURL -Credentials $userCredential -ReturnConnection
	#Set-PnPStorageEntity -Key MicrosoftCustomLearningCdn -Value "https://sharepoint.github.io/sp-custom-learning/v2/" -Description "CDN source for Microsoft Content"
    Get-PnPStorageEntity -Key MicrosoftCustomLearningCdn
    #Set-PnPStorageEntity -Key MicrosoftCustomLearningSite -Value $clSite -Description "Custom Learning Site Collection"
    Get-PnPStorageEntity -Key MicrosoftCustomLearningSite
    #Set-PnPStorageEntity -Key MicrosoftCustomLearningTelemetryOn -Value $optInTelemetry -Description "Custom Learning Telemetry Collection"
    Get-PnPStorageEntity -Key MicrosoftCustomLearningTelemetryOn

   
    Connect-PnPOnline -Url $clSite -Credentials $userCredential
    $clv = Get-PnPListItem -List "Site Pages" -Query "<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>CustomLearningViewer.aspx</Value></Eq></Where></Query></View>"
    if($clv -eq $null){
        $clvPage = Add-PnPClientSidePage "CustomLearningViewer"
        $clvSection = Add-PnPClientSidePageSection -Page $clvPage -SectionTemplate OneColumn -Order 1
        Add-PnPClientSideWebPart -Page $clvPage -Component "Custom Learning for Office 365"
        Set-PnPClientSidePage -Identity $clvPage -Publish
        $clv = Get-PnPListItem -List "Site Pages" -Query "<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>CustomLearningViewer.aspx</Value></Eq></Where></Query></View>"
    }
    $clv["PageLayoutType"] = "SingleWebPartAppPage"
    #$clv["PageLayoutType"] = "Article"
    $clv.Update()
    Invoke-PnPQuery
    
    $cla = Get-PnPListItem -List "Site Pages" -Query "<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>CustomLearningAdmin.aspx</Value></Eq></Where></Query></View>"
    if($cla -eq $null){
        $claPage = Add-PnPClientSidePage "CustomLearningAdmin" -Publish
        $claSection = Add-PnPClientSidePageSection -Page $claPage -SectionTemplate OneColumn -Order 1
        Add-PnPClientSideWebPart -Page $claPage -Component "Custom Learning Admin for Office 365 Web Part"
        Set-PnPClientSidePage -Identity $claPage -Publish
        $cla = Get-PnPListItem -List "Site Pages" -Query "<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>CustomLearningAdmin.aspx</Value></Eq></Where></Query></View>"
    }
    $cla["PageLayoutType"] = "SingleWebPartAppPage"
    #$cla["PageLayoutType"] = "Article"
    $cla.Update()
    Invoke-PnPQuery

}
catch {
	Write-Error "Failed to authenticate to $siteUrl"
	Write-Error $_.Exception
}

