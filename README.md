# UNIGRAM ADS

[![Unity](https://img.shields.io/badge/Unity-2020.1+-2296F3.svg?color=318CE7&style=flat-square&logo=Unity&logoColor=E0FFFF)](https://unity.com/releases/editor/archive)
[![Version](https://img.shields.io/github/package-json/v/Veittech/UnigramAds?color=318CE7&style=flat-square&logo=buffer&logoColor=E0FFFF)](package.json)
[![License](https://img.shields.io/github/license/Veittech/UnigramAds?color=318CE7&style=flat-square&logo=github&logoColor=E0FFFF)](LICENSE)
![Last commit](https://img.shields.io/github/last-commit/Veittech/UnigramAds/master?color=318CE7&style=flat-square&logo=alwaysdata&logoColor=E0FFFF)
![Last release](https://img.shields.io/github/release-date/Veittech/UnigramAds?color=318CE7&style=flat-square&logo=Dropbox&logoColor=E0FFFF)
![Downloads](https://img.shields.io/github/downloads/Veittech/UnigramAds/total?color=318CE7&style=flat-square&logo=codeigniter&logoColor=E0FFFF)

<p align="left">
 <img width="600px" src="https://github.com/Veittech/UnigramAds/blob/master/Assets/sdkBackgroundLogo.png" alt="qr"/>
</p>

**UNIGRAM ADS** is a user-friendly solution for displaying native ads in web applications created on Unity in the format of Telegram Mini Apps.

# Technical Demo

You can test the SDK without installation on a demo app [in the TMA (Telegram Mini App)](https://t.me/UnigramAds_bot/start).

# Supported Networks & Ad Types

|         Ad Networks      | Interstitial  |   Rewarded   |    Banner    |
| ------------------------ | :-----------: | :----------: | :----------: |
| **AdsGram**              | ✔️           | ✔️           | ❌           |
| **AdSonar**              | ✔️           | ✔️           | ⚠️           |

<sub>✔️ Supported</sub> &nbsp; <sub>❌ Not Supported</sub> &nbsp; <sub>⚠️ In progress</sub>

# Installation

**[Download the latest version of the SDK via the .unityPackage file here](https://github.com/Veittech/UnigramAds/releases).**

# Initialization

In the SDK `as of version 1.0.1` access **ONLY MANUAL INITIALIZATION** of one of the selected networks via code.

Below is a test example of what this might look like for one of the available networks:
```c#
public sealed class InitSdkExample: MonoBehaviour
{
    private UnigramAdsSDK _unigramAds;

    private void Start()
    {
        _unigramAds = new UnigramAdsSDK.Builder(
            "your-interstitial-ad-unit", "your-rewarded-ad-unit")
            .WithTestMode()
            .WithAdNetwork(AdNetworkTypes.AdSonar)
            .Build(OnInitializedAdSDK);
    }

    private void OnInitializedAdSDK(bool isSuccess,
        AdNetworkTypes currentNetwork)
    {
        Debug.Log($"Ads sdk innitialized with status: "+
          $"{isSuccess}, ad network: {currentNetwork}";
    }
}
```

Yes, that's how simple the initialization goes, depending on the selected SDK parameters.
Depending on the selected network, you need to create an account there beforehand and add an **EXISTING APPLICATION** under telegram.
Then you need to go through manual moderation there to start showing real ads in your mini app.

 P.S.: [in AdsGram](https://docs.adsgram.ai/publisher/getting-started) you can't use ad blocks until your app is moderated, but [AdSonar](https://docs.adsonar.co/pages/en/integration-guide/introduction) has a test mode.

The `.WithAdNetwork` method specifies the desired ad network that is needed to initialize and display ads through it - `AdsGram` or `AdSonar` are available in this listing **MOMENT OF VERSION 1.0.1**. Yes, in the current version of the SDK, you can initialize **ONLY 1 ADVERTISING NETWORK** and then use it. In future updates I will expand the functionality to allow you to control the display of ads through the selected network.

The `.WithTestMode()` method only adjusts the status of showing messages for debug from the SDK. Due to peculiarities of test mode initialization of one of the networks, this functionality does not work properly yet.

In case you want to initialize **ONLY ONE TYPE** of advertisement to display in your application, you can use the following SDK constructor:

```c#
// Interstitial ad init
var singleAdInit = new UnigramAdsSDK.Builder(
    AdTypes.FullscreenStatic, "your-interstitial-ad-unit")
    .WithAdNetwork(AdNetworkTypes.AdSonar)
    .Build(OnInitializedAdSDK);

// Rewarded ad init
var singleAdInit = new UnigramAdsSDK.Builder(
    AdTypes.RewardedVideo, "your-rewarded-ad-unit")
    .WithAdNetwork(AdNetworkTypes.AdsGram)
    .Build(OnInitializedAdSDK);
```

# Usage Template

Well, after we have initialized the SDK - we can start implementing the display of ads in your mini application. If you have **any questions** when working with the library API, you can always see the implementation of **all available functions** in the Example scene.

## Ad Showing

The implementation of the advertisement display is shown below:
```c#
public sealed class AdShowImplementExample: MonoBehaviour
{
    private UnigramAdsSDK _unigramAds;

    private IVideoAd _interstitialAd;
    private IRewardVideoAd _rewardedAd;

    private void Start()
    {
        _unigramAds = new UnigramAdsSDK.Builder(
            "your-interstitial-ad-unit", "your-rewarded-ad-unit")
            .WithTestMode()
            .WithAdNetwork(AdNetworkTypes.AdSonar)
            .Build(OnInitializedAdSDK);
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd == null)
        {
            return;
        }

        _interstitialAd.Show();
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd == null)
        {
            return;
        }

        _rewardedAd.Show(OnRewardedAdFinished);
    }

    private void OnRewardedAdFinished()
    {
        Debug.Log("Rewarded ad shown, it's time to put the award on!");
    }

    private void OnInitializedAdSDK(bool isSuccess,
        AdNetworkTypes currentNetwork)
    {
        _interstitialAd = new InterstitialAdAdapter();
        _rewardedAd = new RewardAdAdapter();

        Debug.Log($"Ad sdk innitialized with status: "+
          $"{isSuccess}, ad network: {currentNetwork}";
    }
}
```

As you can see, then, reward ads have the ability to immediately get the result of a successful display `by specifying a callback` right in the method!

In case you want to directly subscribe to events about `successful show` or `unsuccessful show` of an ad with an error - then the implementation is as follows:

```c#
public void CreateInterstitial()
{
    IVideoAd interstitialAd = new InterstitialAdAdapter();

    interstitialAd.OnShowFinished += InterstitialAdShown;
    interstitialAd.OnShowFailed += InterstitialAdShowFailed;
}

public void InterstitialAdShown()
{
    Debug.Log("Interstitial ad shown!");
}

public void InterstitialAdShowFailed(string error)
{
    Debug.Warning($"Interstitial ad show failed, reason: {error}");
}
```

The same implementation of result subscription is suitable for `reward ad`, so there's no point in duplicating it here.

## Ad Destroy

I don't know how up to date this implementation is, but based on the ad networks documentation - they `free memory` from an ad unit that is not planned to be used anymore

In this case, I have separate methods for each ad network. In future updates I will change the logic of these methods so that it is **one for all**:
```c#
IVideoAd interstitialAd = new InterstitialAdAdapter();

interstitialAd.Destroy(); // TODO: destroys the ad block from AdsGram
interstitialAd.Destroy("your-interstitial-unit"); // TODO: destroys the ad block from AdSonar
```

The logic for releasing memory from an ad block is **EXACTLY THE SAME** for a rewarded ad.

# Build

Before you start building your unity project in WebGl, you need to do a few things to make sure the library is working properly.

Go to the Build Settings window, then open `Project Settings -> Player -> Resolution and Presentation` and select the `Unigram Ads` build template. To display correctly in Telegram Web View, you need to set `Default Canvas Width` to 1080 and `Default Canvas Height` to 1920, as well as disable the Run in Background option.
These are all the necessary actions that need to be performed for a successful project build and correct operation of the library functions.

<p align="left">
 <img width="600px" src="https://github.com/Veittech/UnigramAds/blob/master/Assets/buildTemplateOverview.png" alt="qr"/>
</p>

In case you specified `AdSonar advertising network` during SDK initialization - you need to make additional settings in the library build template.
Currently, in version 1.0.1, **MANUAL EDITING** of the build template for this ad network is available (in future updates I will solve this problem in an automated way, probably).
Go to the `WebGLTemplates -> UnigramAds` folder and open the `index.html` file to edit this line:
```html
<script src="https://static.sonartech.io/lib/1.0.0/sonar.js?appId=app_aaa2d5da&isDebug=true"></script>
``` 

In it you need to replace the `appId` parameter with the id of your application **from the AdSonar dashboard**.
To activate the test mode, if you have not been moderated yet, you need to leave the `isDebug` parameter at the current value.

Now you can build your project and test your ad display implementation!

# Donations

Ton Wallet (TON/NOT/USDt):
```
UQDPwEk-cnQXEfFaaNVXywpbKACUMwVRupkgWjhr_f4Ursw6
```

Multichain Wallet (BTC/ETH/BNB/MATIC)
```
0x231803Df809C207FaA330646BB5547fD087FEcA1
```

# Support

[![Email](https://img.shields.io/badge/-gmail-090909?style=for-the-badge&logo=gmail)](https://mail.google.com/mail/?view=cm&fs=1&to=misster.veit@gmail.com)
[![Telegram](https://img.shields.io/badge/-Telegram-090909?style=for-the-badge&logo=telegram)](https://t.me/unigram_tools)
