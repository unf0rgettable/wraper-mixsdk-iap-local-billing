using System;
using System.Collections.Generic;
using System.Linq;
using LittleBit;
using LittleBit.Modules.IAppModule.Data.ProductWrappers;
using LittleBit.Modules.IAppModule.Data.Purchases;
using LittleBit.Modules.IAppModule.Services;
using LittleBitGames.Environment.Events;
using MixNameSpace;
using UnityEngine;
using UnityEngine.Scripting;

public class MixIAPService : IIAPService,IIAPRevenueEvent
{
    //ToDo понять что это такое)
    private const string CartType = "Shop";
    private const string Signature = "VVO";
    private const string ItemType = "Offer";
    
    private readonly List<OfferConfig> _offerConfigs;
    private readonly InitIap _initIap;
    private readonly List<string> _boughtProducts;
    private readonly IAPService.ProductCollections _productCollection;
    private MixSDKConfig _mixSDKConfig;
    public event Action<string> OnPurchasingSuccess;
    public event Action<string> OnPurchasingFailed;
    public event Action OnInitializationComplete;
    public bool IsInitialized => _initIap != null ? _initIap.IsInit : false;

    [Preserve]
    public MixIAPService(List<OfferConfig> offerConfigs, InitIap initIap)
    {
        _offerConfigs = offerConfigs;
        _initIap = initIap;
        _boughtProducts = new List<string>();
        _productCollection = new IAPService.ProductCollections();
    }

    public void Init(MixSDKConfig mixSDKConfig)
    {
        _initIap.OnCompleteInit +=  OnInit;
        _initIap.Init();
        _mixSDKConfig = mixSDKConfig;
        foreach (var offer in _offerConfigs)
        {
            if (_mixSDKConfig.mixInput.items.FirstOrDefault(v => v.itemId == offer.Id) == null)
            {
                Debug.LogError($"Offer with id {offer.Id} not contains in mixSdkConfig");
            }
            _productCollection.AddConfig(offer);
        }
        
        MixIap.instance.SetAction((e) =>
        {
            if (e.itemType == ProductType.Consumable)
            {
                MixIap.instance.FinishPurchase(e);
            }
            else if(e.itemType == ProductType.NonConsumable)
            {
                
                MixIap.instance.GetAllNonConsumable();
            }
            
#if !UNITY_EDITOR && !IAP_DEBUG
            var product = GetRuntimeProductWrapper(e.itemId) as RuntimeProductWrapper;
            product?.Purchase();            
#endif
            OnPurchasingSuccess?.Invoke(e.itemId);
            //send item
            //MixIap.instance.FinishPurchase(e.purchasedProduct.definition.id);
        });
    }

    private void OnInit()
    {
        //_productCollection.AddUnityIAPProductCollection(Purchaser.Instance.StoreController.products);

        OnInitializationComplete?.Invoke();
    }

    public void Purchase(string id, bool freePurchase = false)
    {
#if IAP_DEBUG || UNITY_EDITOR
        var product = (GetProductWrapper(id) as EditorProductWrapper);

        if (product is null) return;
            
        if (!product.Metadata.CanPurchase) return;
            
        product!.Purchase();
        OnPurchasingSuccess?.Invoke(id);
        PurchasingProductSuccess(id);
#else

            if (freePurchase)
            {
                var product = GetRuntimeProductWrapper(id) as RuntimeProductWrapper;

                product?.Purchase();
                OnPurchasingSuccess?.Invoke(id);
                PurchasingProductSuccess(id);
                return;
            }
            
            MixIap.instance.PurchaseItem(id, (value)=>
            {
                OnPurchasingFailed?.Invoke(value);
            });
#endif
    }

    public void RestorePurchasedProducts(Action<bool> callback)
    {
        MixIap.instance.AppleRestore(callback);
    }

    public IProductWrapper GetProductWrapper(string id)
    {
#if IAP_DEBUG || UNITY_EDITOR
        return GetDebugProductWrapper(id);
#else
            try
            {
                return GetRuntimeProductWrapper(id);
            }
            catch
            {
                Debug.LogError($"Can't create runtime product wrapper with id:{id}");
                return null;
            }
#endif
    }

    private IProductWrapper GetDebugProductWrapper(string id)
    {
        return _productCollection.GetEditorProductWrapper(id);
    }

    private IProductWrapper GetRuntimeProductWrapper(string id)
    {
        var itemInfo = _mixSDKConfig.mixInput.items.FirstOrDefault(i => i.itemId == id);
        float price = 0.0F;
        float.TryParse(itemInfo.usdPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price);
        return new RuntimeProductWrapper(
            new ProductParams(
                itemInfo.itemId,
                itemInfo.type,
                (decimal)price));
    }

    public event Action<IDataEventEcommerce> OnPurchasingProductSuccess;
    
    private void PurchasingProductSuccess(string productId)
    {
        var product = GetProductWrapper(productId);
        var metadata = product.Metadata;
        var definition = product.Definition;
        var receipt = product.TransactionData.Receipt;

        var data = new DataEventEcommerce(
            metadata.CurrencyCode,
            (double) metadata.LocalizedPrice,
            ItemType, definition.Id,
            CartType, receipt,
            Signature);       
        OnPurchasingProductSuccess?.Invoke(data);
    }
}
