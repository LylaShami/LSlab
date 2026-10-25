using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using TALXIS.DevKit.Build.Dataverse.Plugin;

namespace Plugins.Warehouse
{
    [PluginStep("Create", "lab_warehousetransaction", PluginStage.PreValidation)]
    public class ValidateWarehouseTransactionPlugin : PluginBase
    {
        public ValidateWarehouseTransactionPlugin()
            : this(null, null)
        {
        }

        public ValidateWarehouseTransactionPlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(ValidateWarehouseTransactionPlugin))
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            var context = localPluginContext.PluginExecutionContext;
            var tracingService = localPluginContext.TracingService;

            try
            {
                if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target) || target.LogicalName != "lab_warehousetransaction")
                {
                    return;
                }

                if (!target.Contains("lab_quantity") || !target.Contains("lab_itemid") || !target.Contains("lab_transactiontype"))
                {
                    return;
                }

                var transactionType = target.GetAttributeValue<OptionSetValue>("lab_transactiontype");
                if (transactionType == null || transactionType.Value != 100000001)
                {
                    return;
                }

                var itemReference = target.GetAttributeValue<EntityReference>("lab_itemid");
                if (itemReference == null)
                {
                    return;
                }

                var quantity = target.GetAttributeValue<int>("lab_quantity");
                var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);
                var item = service.Retrieve("lab_warehouseitem", itemReference.Id, new ColumnSet("lab_availablequantity"));
                var available = item.GetAttributeValue<int>("lab_availablequantity");

                if (quantity > available)
                {
                    throw new InvalidPluginExecutionException(
                        $"Not enough product in stock. Available: {available}, requested: {quantity}.");
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception exception)
            {
                tracingService.Trace("ValidateWarehouseTransactionPlugin failed: {0}", exception);
                throw;
            }
        }
    }
}