using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EasyfisShop.ApiControllers
{
    public class ApiTrnShopOrderController : ApiController
    {
        // ============
        // Data Context
        // ============
        public Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        // ===============
        // List Shop Order
        // ===============
        [Authorize, HttpGet, Route("api/shopOrder/list/{startDate}/{endDate}/{shopGroupId}/{shopOrderStatusId}")]
        public List<Entities.TrnShopOrder> ListShopOrder(String startDate, String endDate, String shopGroupId, String shopOrderStatusId)
        {
            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            var shopOrders = from d in db.TrnShopOrders
                             where d.BranchId == currentUser.FirstOrDefault().BranchId
                             && d.SPDate >= Convert.ToDateTime(startDate)
                             && d.SPDate <= Convert.ToDateTime(endDate)
                             && d.ShopGroupId == Convert.ToInt32(shopGroupId)
                             && d.ShopOrderStatusId == Convert.ToInt32(shopOrderStatusId)
                             select new Entities.TrnShopOrder
                             {
                                 Id = d.Id,
                                 SPDate = d.SPDate.ToShortDateString(),
                                 SPNumber = d.SPNumber,
                                 Item = d.MstArticle.Article,
                                 Quantity = d.Quantity,
                                 Amount = d.Amount,
                                 Particulars = d.Particulars,
                                 IsLocked = d.IsLocked,
                                 CreatedBy = d.MstUser.FullName,
                                 CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                 UpdatedBy = d.MstUser1.FullName,
                                 UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                             };

            return shopOrders.OrderByDescending(d => d.Id).ToList();
        }

        // =================
        // Detail Shop Order
        // =================
        [Authorize, HttpGet, Route("api/shopOrder/detail/{id}")]
        public Entities.TrnShopOrder DetailShopOrder(String id)
        {
            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            var shopOrder = from d in db.TrnShopOrders
                            where d.BranchId == currentUser.FirstOrDefault().BranchId
                            && d.Id == Convert.ToInt32(id)
                            select new Entities.TrnShopOrder
                            {
                                Id = d.Id,
                                Branch = d.MstBranch.Branch,
                                SPDate = d.SPDate.ToShortDateString(),
                                SPNumber = d.SPNumber,
                                ItemId = d.ItemId,
                                Quantity = d.Quantity,
                                UnitId = d.UnitId,
                                Amount = d.Amount,
                                ShopOrderStatusId = d.ShopOrderStatusId,
                                ShopOrderStatusDate = d.ShopOrderStatusDate.ToShortDateString(),
                                ShopGroupId = d.ShopGroupId,
                                Particulars = d.Particulars,
                                Status = d.Status,
                                IsLocked = d.IsLocked,
                                CreatedBy = d.MstUser.FullName,
                                CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                                UpdatedBy = d.MstUser1.FullName,
                                UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                            };

            return shopOrder.FirstOrDefault();
        }

        // ============================
        // Dropdown List - Item (Field)
        // ============================
        [Authorize, HttpGet, Route("api/shopOrder/dropdown/list/item")]
        public List<Entities.MstItem> DropdownListShopOrderItem()
        {
            var items = from d in db.MstArticles.OrderBy(d => d.Article)
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        select new Entities.MstItem
                        {
                            Id = d.Id,
                            Item = d.Article,
                            Code = d.ArticleCode,
                            ManualCode = d.ManualArticleCode,
                            UnitId = d.UnitId
                        };

            return items.OrderByDescending(d => d.Item).ToList();
        }

        // ============================
        // Dropdown List - Unit (Field)
        // ============================
        [Authorize, HttpGet, Route("api/shopOrder/dropdown/list/unit")]
        public List<Entities.MstUnit> DropdownListShopOrderUnit()
        {
            var units = from d in db.MstUnits.OrderBy(d => d.Unit)
                        select new Entities.MstUnit
                        {
                            Id = d.Id,
                            Unit = d.Unit
                        };

            return units.OrderByDescending(d => d.Id).ToList();
        }

        // =========================================
        // Dropdown List - Shop Order Status (Field)
        // =========================================
        [Authorize, HttpGet, Route("api/shopOrder/dropdown/list/shopOrderStatus")]
        public List<Entities.MstShopOrderStatus> DropdownListShopOrderShopOrderStatus()
        {
            var shopOrderStatuses = from d in db.MstShopOrderStatus
                                    select new Entities.MstShopOrderStatus
                                    {
                                        Id = d.Id,
                                        ShopOrderStatusCode = d.ShopOrderStatusCode,
                                        ShopOrderStatus = d.ShopOrderStatus
                                    };

            return shopOrderStatuses.ToList();
        }

        // ========================================
        // Dropdown List - Shop Order Group (Field)
        // ========================================
        [Authorize, HttpGet, Route("api/shopOrder/dropdown/list/shopOrderGroup")]
        public List<Entities.MstShopGroup> DropdownListShopOrderShopOrderGroup()
        {
            var shopGroups = from d in db.MstShopGroups
                             select new Entities.MstShopGroup
                             {
                                 Id = d.Id,
                                 ShopGroupCode = d.ShopGroupCode,
                                 ShopGroup = d.ShopGroup
                             };

            return shopGroups.ToList();
        }

        // ==============================
        // Dropdown List - Status (Field)
        // ==============================
        [Authorize, HttpGet, Route("api/shopOrder/dropdown/list/status")]
        public List<Entities.MstStatus> DropdownListShopOrderStatus()
        {
            var statuses = from d in db.MstStatus
                           where d.Category.Equals("SP")
                           select new Entities.MstStatus
                           {
                               Id = d.Id,
                               Status = d.Status
                           };

            return statuses.OrderByDescending(d => d.Id).ToList();
        }

        // ===================
        // Fill Leading Zeroes
        // ===================
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0) { result = '0' + result; pad--; }

            return result;
        }

        // =================
        // Import Shop Group
        // =================
        [Authorize, HttpPost, Route("api/shopOrder/import")]
        public HttpResponseMessage ImportShopOrder(List<Entities.TrnShopOrder> objShopOrders)
        {
            try
            {
                HttpStatusCode responseStatusCode = HttpStatusCode.OK;
                String responseMessage = "";

                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                var userForm = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals("ShopOrderList") select d;

                if (!userForm.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "No rights."; }
                else if (!userForm.FirstOrDefault().CanAdd) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "No add rights."; }
                else
                {
                    Debug.WriteLine(objShopOrders);

                    if (objShopOrders.Any())
                    {
                        Int32 count = 1;

                        List<Entities.TrnShopOrder> newShopOrders = new List<Entities.TrnShopOrder>();
                        foreach (var objShopOrder in objShopOrders)
                        {
                            Boolean isValid = true;

                            var shopOrderObjects = from d in objShopOrders
                                                   where d.SPNumber.Equals(objShopOrder.SPNumber)
                                                   select d;

                            if (shopOrderObjects.Any())
                            {
                                if (shopOrderObjects.Count() > 1)
                                {
                                    isValid = false;

                                    responseStatusCode = HttpStatusCode.NotFound;
                                    responseMessage = "Duplicate order number: " + objShopOrder.SPNumber;

                                    newShopOrders = new List<Entities.TrnShopOrder>();
                                    break;
                                }
                            }

                            Int32 itemId = 0, unitId = 0;
                            var item = from d in db.MstArticles
                                       where d.ManualArticleCode.Equals(objShopOrder.ItemCode)
                                       && d.ArticleTypeId == 1
                                       && d.IsLocked == true
                                       select d;

                            if (item.Any())
                            {
                                itemId = item.FirstOrDefault().Id;
                                unitId = item.FirstOrDefault().UnitId;
                            }
                            else
                            {
                                isValid = false;

                                responseStatusCode = HttpStatusCode.NotFound;
                                responseMessage = "Item code: " + objShopOrder.ItemCode + " not found.";

                                newShopOrders = new List<Entities.TrnShopOrder>();
                                break;
                            }

                            Int32 shopOrderStatusId = 0;
                            var shopOrderStatus = from d in db.MstShopOrderStatus select d;
                            if (shopOrderStatus.Any())
                            {
                                shopOrderStatusId = shopOrderStatus.FirstOrDefault().Id;
                            }
                            else
                            {
                                isValid = false;

                                responseStatusCode = HttpStatusCode.NotFound;
                                responseMessage = "No shop order status";

                                newShopOrders = new List<Entities.TrnShopOrder>();
                                break;
                            }

                            Int32 shopGroupId = 0;
                            var shopGroup = from d in db.MstShopGroups select d;
                            if (shopGroup.Any())
                            {
                                shopGroupId = shopGroup.FirstOrDefault().Id;
                            }
                            else
                            {
                                isValid = false;

                                responseStatusCode = HttpStatusCode.NotFound;
                                responseMessage = "No shop group";

                                newShopOrders = new List<Entities.TrnShopOrder>();
                                break;
                            }

                            var order = from d in db.TrnShopOrders
                                        where d.SPNumber.Equals(objShopOrder.SPNumber)
                                        && d.IsLocked == true
                                        select d;

                            if (order.Any())
                            {
                                isValid = false;

                                responseStatusCode = HttpStatusCode.NotFound;
                                responseMessage = "Shop order number: " + objShopOrder.SPNumber + " is already exist.";

                                newShopOrders = new List<Entities.TrnShopOrder>();
                                break;
                            }

                            if (isValid)
                            {
                                newShopOrders.Add(new Entities.TrnShopOrder()
                                {
                                    BranchId = currentUser.FirstOrDefault().BranchId,
                                    SPNumber = objShopOrder.SPNumber,
                                    SPDate = objShopOrder.SPDate,
                                    ItemId = itemId,
                                    Quantity = objShopOrder.Quantity,
                                    UnitId = unitId,
                                    Amount = objShopOrder.Amount,
                                    ShopOrderStatusId = shopOrderStatusId,
                                    ShopOrderStatusDate = objShopOrder.SPDate,
                                    ShopGroupId = shopGroupId,
                                    Particulars = objShopOrder.Particulars,
                                    Status = null,
                                    IsPrinted = false,
                                    IsLocked = true,
                                    CreatedById = currentUser.FirstOrDefault().Id,
                                    CreatedDateTime = DateTime.Now.ToShortDateString(),
                                    UpdatedById = currentUser.FirstOrDefault().Id,
                                    UpdatedDateTime = DateTime.Now.ToShortDateString()
                                });

                                count += 1;
                            }
                        }

                        if ((count - 1) == objShopOrders.Count())
                        {
                            if (newShopOrders.Any())
                            {
                                foreach (var objNewShopOrder in newShopOrders)
                                {
                                    Data.TrnShopOrder newShopOrder = new Data.TrnShopOrder
                                    {
                                        BranchId = objNewShopOrder.BranchId,
                                        SPNumber = objNewShopOrder.SPNumber,
                                        SPDate = Convert.ToDateTime(objNewShopOrder.SPDate),
                                        ItemId = objNewShopOrder.ItemId,
                                        Quantity = objNewShopOrder.Quantity,
                                        UnitId = objNewShopOrder.UnitId,
                                        Amount = objNewShopOrder.Amount,
                                        ShopOrderStatusId = objNewShopOrder.ShopOrderStatusId,
                                        ShopOrderStatusDate = Convert.ToDateTime(objNewShopOrder.ShopOrderStatusDate),
                                        ShopGroupId = objNewShopOrder.ShopGroupId,
                                        Particulars = objNewShopOrder.Particulars,
                                        Status = objNewShopOrder.Status,
                                        IsPrinted = objNewShopOrder.IsPrinted,
                                        IsLocked = objNewShopOrder.IsLocked,
                                        CreatedById = objNewShopOrder.CreatedById,
                                        CreatedDateTime = Convert.ToDateTime(objNewShopOrder.CreatedDateTime),
                                        UpdatedById = objNewShopOrder.UpdatedById,
                                        UpdatedDateTime = Convert.ToDateTime(objNewShopOrder.UpdatedDateTime),
                                    };

                                    db.TrnShopOrders.InsertOnSubmit(newShopOrder);
                                }

                                db.SubmitChanges();
                            }
                        }
                    }
                }

                return Request.CreateResponse(responseStatusCode, responseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ==============
        // Add Shop Order
        // ==============
        [Authorize, HttpPost, Route("api/shopOrder/add")]
        public HttpResponseMessage AddShopOrder(Entities.TrnShopOrder objShopOrder)
        {
            try
            {
                HttpStatusCode responseStatusCode = HttpStatusCode.OK;
                String responseMessage = "";

                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                var userForm = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals("ShopOrderList") select d;
                var item = from d in db.MstArticles where d.ArticleTypeId == 1 && d.IsLocked == true select d;
                IQueryable<Data.MstUnit> unit = null;
                if (item.Any())
                {
                    unit = from d in db.MstUnits where d.Id == item.FirstOrDefault().UnitId select d;
                }
                var shopOrderStatus = from d in db.MstShopOrderStatus select d;
                var shopGroup = from d in db.MstShopGroups select d;

                if (!userForm.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "No rights."; }
                else if (!userForm.FirstOrDefault().CanAdd) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "No add rights."; }
                else if (!item.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Item not found."; }
                else if (!unit.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Unit not found."; }
                else if (!shopOrderStatus.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop order status not found."; }
                else if (!shopGroup.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop group not found."; }
                else
                {
                    Data.TrnShopOrder newShopOrder = new Data.TrnShopOrder
                    {
                        BranchId = currentUser.FirstOrDefault().BranchId,
                        SPNumber = "NA",
                        SPDate = DateTime.Today,
                        ItemId = item.OrderByDescending(d => d.Article).FirstOrDefault().Id,
                        Quantity = 0,
                        UnitId = unit.FirstOrDefault().Id,
                        Amount = 0,
                        ShopOrderStatusId = shopOrderStatus.FirstOrDefault().Id,
                        ShopOrderStatusDate = DateTime.Today,
                        ShopGroupId = shopGroup.FirstOrDefault().Id,
                        Particulars = "NA",
                        Status = null,
                        IsPrinted = false,
                        IsLocked = false,
                        CreatedById = currentUser.FirstOrDefault().Id,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUser.FirstOrDefault().Id,
                        UpdatedDateTime = DateTime.Now
                    };

                    db.TrnShopOrders.InsertOnSubmit(newShopOrder);
                    db.SubmitChanges();

                    responseMessage = newShopOrder.Id.ToString();
                }

                return Request.CreateResponse(responseStatusCode, responseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===============
        // Save Shop Order
        // ===============
        [Authorize, HttpPut, Route("api/shopOrder/save")]
        public HttpResponseMessage SaveShopOrder(Entities.TrnShopOrder objShopOrder)
        {
            try
            {
                HttpStatusCode responseStatusCode = HttpStatusCode.OK;
                String responseMessage = "";

                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                var userForm = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals("ShopOrderDetail") select d;
                var shopOrder = from d in db.TrnShopOrders where d.Id == objShopOrder.Id select d;
                var item = from d in db.MstArticles where d.Id == objShopOrder.ItemId select d;
                var unit = from d in db.MstUnits where d.Id == objShopOrder.UnitId select d;
                var shopOrderStatus = from d in db.MstShopOrderStatus where d.Id == objShopOrder.ShopOrderStatusId select d;
                var shopGroup = from d in db.MstShopGroups where d.Id == objShopOrder.ShopGroupId select d;
                var order = from d in db.TrnShopOrders where d.SPNumber.Equals(objShopOrder.SPNumber) && d.IsLocked == true select d;

                if (!userForm.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "No rights."; }
                else if (!userForm.FirstOrDefault().CanLock) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "No lock rights."; }
                else if (!shopOrder.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Reference not found."; }
                else if (shopOrder.FirstOrDefault().IsLocked) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "Already locked."; }
                else if (!item.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Item not found."; }
                else if (!unit.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Unit not found."; }
                else if (!shopOrderStatus.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop order status not found."; }
                else if (!shopGroup.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop group not found."; }
                else if (order.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop order number: " + objShopOrder.SPNumber + " is already exist."; }
                else
                {
                    var saveShopOrder = shopOrder.FirstOrDefault();
                    saveShopOrder.SPDate = Convert.ToDateTime(objShopOrder.SPDate);
                    saveShopOrder.SPNumber = objShopOrder.SPNumber;
                    saveShopOrder.ItemId = item.FirstOrDefault().Id;
                    saveShopOrder.Quantity = objShopOrder.Quantity;
                    saveShopOrder.UnitId = unit.FirstOrDefault().Id;
                    saveShopOrder.Amount = objShopOrder.Amount;
                    saveShopOrder.ShopOrderStatusId = shopOrderStatus.FirstOrDefault().Id;
                    saveShopOrder.ShopOrderStatusDate = Convert.ToDateTime(objShopOrder.ShopOrderStatusDate);
                    saveShopOrder.ShopGroupId = shopGroup.FirstOrDefault().Id;
                    saveShopOrder.Particulars = objShopOrder.Particulars;
                    saveShopOrder.UpdatedById = currentUser.FirstOrDefault().Id;
                    saveShopOrder.UpdatedDateTime = DateTime.Now;
                    db.SubmitChanges();
                }

                return Request.CreateResponse(responseStatusCode, responseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // ===============
        // Lock Shop Order
        // ===============
        [Authorize, HttpPut, Route("api/shopOrder/lock")]
        public HttpResponseMessage LockShopOrder(Entities.TrnShopOrder objShopOrder)
        {
            try
            {
                HttpStatusCode responseStatusCode = HttpStatusCode.OK;
                String responseMessage = "";

                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                var userForm = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals("ShopOrderDetail") select d;
                var shopOrder = from d in db.TrnShopOrders where d.Id == objShopOrder.Id select d;
                var item = from d in db.MstArticles where d.Id == objShopOrder.ItemId select d;
                var unit = from d in db.MstUnits where d.Id == objShopOrder.UnitId select d;
                var shopOrderStatus = from d in db.MstShopOrderStatus where d.Id == objShopOrder.ShopOrderStatusId select d;
                var shopGroup = from d in db.MstShopGroups where d.Id == objShopOrder.ShopGroupId select d;
                var order = from d in db.TrnShopOrders where d.SPNumber.Equals(objShopOrder.SPNumber) && d.IsLocked == true select d;

                if (!userForm.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "No rights."; }
                else if (!userForm.FirstOrDefault().CanLock) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "No lock rights."; }
                else if (!shopOrder.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Reference not found."; }
                else if (shopOrder.FirstOrDefault().IsLocked) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "Already locked."; }
                else if (!item.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Item not found."; }
                else if (!unit.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Unit not found."; }
                else if (!shopOrderStatus.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop order status not found."; }
                else if (!shopGroup.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop group not found."; }
                else if (order.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Shop order number: " + objShopOrder.SPNumber + " is already exist."; }
                else
                {
                    var lockShopOrder = shopOrder.FirstOrDefault();
                    lockShopOrder.SPDate = Convert.ToDateTime(objShopOrder.SPDate);
                    lockShopOrder.SPNumber = objShopOrder.SPNumber;
                    lockShopOrder.ItemId = item.FirstOrDefault().Id;
                    lockShopOrder.Quantity = objShopOrder.Quantity;
                    lockShopOrder.UnitId = unit.FirstOrDefault().Id;
                    lockShopOrder.Amount = objShopOrder.Amount;
                    lockShopOrder.ShopOrderStatusId = shopOrderStatus.FirstOrDefault().Id;
                    lockShopOrder.ShopOrderStatusDate = Convert.ToDateTime(objShopOrder.ShopOrderStatusDate);
                    lockShopOrder.ShopGroupId = shopGroup.FirstOrDefault().Id;
                    lockShopOrder.Particulars = objShopOrder.Particulars;
                    lockShopOrder.IsLocked = true;
                    lockShopOrder.UpdatedById = currentUser.FirstOrDefault().Id;
                    lockShopOrder.UpdatedDateTime = DateTime.Now;
                    db.SubmitChanges();
                }

                return Request.CreateResponse(responseStatusCode, responseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // =================
        // Unlock Shop Order
        // =================
        [Authorize, HttpPut, Route("api/shopOrder/unlock")]
        public HttpResponseMessage UnlockShopOrder(String id)
        {
            try
            {
                HttpStatusCode responseStatusCode = HttpStatusCode.OK;
                String responseMessage = "";

                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                var userForm = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals("ShopOrderDetail") select d;
                var shopOrder = from d in db.TrnShopOrders where d.Id == Convert.ToInt32(id) select d;

                if (!userForm.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "No rights."; }
                else if (!userForm.FirstOrDefault().CanUnlock) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "No unlock rights."; }
                else if (!shopOrder.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Reference not found."; }
                else if (!shopOrder.FirstOrDefault().IsLocked) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "Already unlocked."; }
                else
                {
                    var unlockShopOrder = shopOrder.FirstOrDefault();
                    unlockShopOrder.IsLocked = false;
                    unlockShopOrder.UpdatedById = currentUser.FirstOrDefault().Id;
                    unlockShopOrder.UpdatedDateTime = DateTime.Now;
                    db.SubmitChanges();
                }

                return Request.CreateResponse(responseStatusCode, responseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // =================
        // Delete Shop Order
        // =================
        [Authorize, HttpDelete, Route("api/shopOrder/delete")]
        public HttpResponseMessage DeleteShopOrder(String id)
        {
            try
            {
                HttpStatusCode responseStatusCode = HttpStatusCode.OK;
                String responseMessage = "";

                var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
                var userForm = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals("ShopOrderList") select d;
                var shopOrder = from d in db.TrnShopOrders where d.Id == Convert.ToInt32(id) select d;

                if (!userForm.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "No rights."; }
                else if (!userForm.FirstOrDefault().CanDelete) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "No delete rights."; }
                else if (!shopOrder.Any()) { responseStatusCode = HttpStatusCode.NotFound; responseMessage = "Reference not found."; }
                else if (shopOrder.FirstOrDefault().IsLocked) { responseStatusCode = HttpStatusCode.BadRequest; responseMessage = "Cannot delete locked shop order."; }
                else
                {
                    db.TrnShopOrders.DeleteOnSubmit(shopOrder.FirstOrDefault());
                    db.SubmitChanges();
                }

                return Request.CreateResponse(responseStatusCode, responseMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
