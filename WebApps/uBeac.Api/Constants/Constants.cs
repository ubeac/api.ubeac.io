namespace uBeac.Api
{
    public class AccessConstants
    {
        public const string DESCRIPTION = "User access level management in a team";
        public const string ADD_SUMMARY = "Assign access to a user for the team"; 
        public const string ADD_DESCRIPTION = "* Access levels are: 0 (View), 1 (Admin) \n * Returns added access id \n * Requires admin access level to the team";
        public const string UPDATE_SUMMARY = "Updates an existing access level for a user on a team";
        public const string UPDATE_DESCRIPTION = "* Access levels are: 0 (View), 1 (Admin) \n * Requires admin privileges, AccessId and new access level \n * Returns a boolean that shows the related access level was updated or not";
        public const string DELETE_SUMMARY = "Deletes an existing access for a user on a team";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges and AccessId \n * You can not delete the last access of a team \n * Each team should have at least one Admin access \n * Returns a boolean that shows the related access level was updated or not"; 
    }

    public class TeamConstants
    {
        public const string DESCRIPTION = "Team management"; 
        public const string GETALL_SUMMARY = "Returns all Teams that a user have access"; 
        public const string GETALL_DESCRIPTION = "* Requires view privileges";
        public const string GETBYID_SUMMARY = "Returns all data and entities related to the team";
        public const string GETBYID_DESCRIPTION = "* Requires view privileges \n * Requires TeamId";
        public const string ADD_SUMMARY = "Creates a new team";
        public const string ADD_DESCRIPTION = "* Namespace is required, should be unique and should contains lower-case letters and/or digits between 6 to 50 characters (we convert capital letters to lower-case) \n * Namespace cannot start with 'momentaj' or 'ubeac'  \n * Attribute property can be anything you need to have \n * Returns id of the added team \n * By adding a team, an access will be added automatically for your user to the team \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string UPDATE_SUMMARY = "Updates an existing team";
        public const string UPDATE_DESCRIPTION = "* Requires admin privileges to that team \n * TeamId, name and namespace are required \n * Namespace cannot start with 'momentaj' or 'ubeac' \n * Returns a boolean that shows the action is done successfully or not \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string DELETE_SUMMARY = "Deletes an existing team with all related data";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges and TeamId \n * You can not delete the last team \n * Each user must have at least one team \n * Returns a boolean that shows the action is done successfully or not \n * By deleting the team, all related data will be deleted.";
        public const string EXISTS_SUMMARY = "Checks namespace existans";
        public const string EXISTS_DESCRIPTION = "Checks if a team with this namespace exists or not";
        public const string ACCESSTOKEN = "AccessToken";
        public const string TEAM_ID = "teamId";
        public const string IDENTITY_CLAIM = "ApiToken";
        public const string GENERATE_TOKEN_SUMMARY = "";
        public const string GENERATE_TOKEN_DESCRIPTION = "";
        public const string ACCESS_LEVEL = "accessLevel";
    }

    public class EntityConstants
    {        
        public const string ADD_SUMMARY = "Adds a new entity";
        public const string ADD_DESCRIPTION = "* Requires admin privileges to the team \n * TeamId and name are required \n * Attribute property can contains any additional data related to the entity \n * Returns id of the added entity \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string UPDATE_SUMMARY = "Updates an existing entity";
        public const string UPDATE_DESCRIPTION = "* Requires admin privileges to that team \n * TeamId and name are required \n * Returns a boolean that shows the action is done successfully or not \n * Attribute property can contains any additional data related to the entity \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string DELETE_SUMMARY = "Deletes an existing entity with all related data";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges and entityId \n * Returns a boolean that shows the action is done successfully or not";
    }

    public class BuildingConstants
    {
        public const string DESCRIPTION = "Building management";
        public const string DELETE_SUMMARY = "Deletes a building and all of its floors";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges to that team \n * BuildingId is required \n * Returns a boolean that shows the action is done successfully or not \n * Deletes all building's floors \n * Updates floorId property to null for all gateways that are located in those deleted floors \n * Deletes deviceSummaries related to those floors";
    }

    public class DashboardConstants
    {
        public const string DESCRIPTION = "Dashboard management";
        public const string UPDATE_WIDGETS_SUMMARY = "Updates all widgetes related to the dashboard";
        public const string UPDATE_WIDGETS_DESCRIPTION = "* Requires admin privileges to that team \n * TeamId and dashboardId are required \n * Returns a boolean that shows the action is done successfully or not \n * You need to pass widgets as a list of widget \n * It will remove dashboard's old widgetes and insert new widgets \n * X, Y properties define the position of each widget in dashboard, it is based on our griding system and it is a digit between 0 - 11 \n * W, H define width and height of a widget in dashboard. It should be a digit between 1-12";
        public const string DELETE_SUMMARY = "Deletes a dashboard and all its widgets";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges to that team \n * DashboardId is required \n * Returns a boolean that shows the action is done successfully or not \n * Deletes all dashboard's widgets";
    }

    public class DeviceConstants
    {
        public const string DESCRIPTION = "Device management";
        public const string ADD_SUMMARY = "Adds a new device";
        public const string ADD_DESCRIPTION = "* Requires admin privileges to the team \n * TeamId and name are required \n * Attribute property can contains any additional data related to the entity \n * Returns id of the added entity \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters \n * Uid property is required and should be unique in the team \n * Uid property should be maximum 50 characters \n * FloorId defines the floor that device is located in \n * X, Y are coordinates of gateway position on map based on percent. e.g. 20 and 20 means 20 percent down and 20 percent right from top left point";
        public const string UPDATE_SUMMARY = "Updates an existing device";
        public const string UPDATE_DESCRIPTION = "* Requires admin privileges to that team \n * TeamId and name are required \n * Returns a boolean that shows the action is done successfully or not \n * Attribute property can contains any additional data related to the entity \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters \n * FloorId defines the floor that device is located in \n * X, Y are coordinates of gateway position on map based on percent. e.g. 20 and 20 means 20 percent down and 20 percent right from top left point";
        public const string DELETE_SUMMARY = "Deletes a device and all of its sensors";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges to that team \n * DeviceId is required \n * Returns a boolean that shows the action is done successfully or not \n * Deletes the device summary data, all device's sensors and all sensors data";
    }

    public class FloorConstants
    {
        public const string DESCRIPTION = "Floor management";
        public const string ADD_SUMMARY = "Adds a new floor";
        public const string ADD_DESCRIPTION = "* Requires admin privileges to the team \n * TeamId, buildingId and name are required \n * Attribute property can contains any additional data related to the floor \n * Returns id of the added floor \n * PlanFileId is the file id related to floor plan that is generated after uploading the related file \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string UPDATE_SUMMARY = "Updates an existing floor";
        public const string UPDATE_DESCRIPTION = "* Requires admin privileges to that team \n * TeamId, buildingId and name are required \n * Returns a boolean that shows the action is done successfully or not \n * Attribute property can contains any additional data related to the entity \n * PlanFileId is the file id related to floor plan that is generated after uploading the related file \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string DELETE_SUMMARY = "Deletes a floor";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges to that team \n * FloorId is required \n * Returns a boolean that shows the action is done successfully or not \n * Deletes the device summary data related to that floor and Updates floorId property to null for all gateways that are located in that floor ";
    }

    public class GatewayConstants
    {
        public const string DESCRIPTION = "Gateway management";
        public const string ADD_SUMMARY = "Adds a new gateway";
        public const string ADD_DESCRIPTION = "* Requires admin privileges to the team \n * TeamId, name, firmwareId and url are required \n * Languages are: 0 (NA), 1 (JavaScript) \n * Attribute property can contains any additional data related to the gateway \n * Returns id of the added gateway \n * FloorId is the floor that the gateway is located in \n * Security contains different parts for custom HTTP headers, defining to accept requests only over SSL or TLS, defining IP restriction and username and password for MQTT \n * In security section, Http.Headers should be like this, {'customKey': 'customValue'} \n * In security section, ipRestriction contains two list of Ips, one for Allowed ips and one for denied ips. It also accepts range Ips like 192.168.0.1-10 or 192.168.0.1/24  \n * You cannot use * as wildcard in ipRestriction, e.g. 192.168.0.* is not allowed  \n * Url should be unique in the namespace and should be alphanumeric \n * X, Y are coordinates of gateway position on map based on percent. e.g. 20 and 20 means 20 percent down and 20 percent right from top left point \n * Gateway url cannot start with momentaj and ubeac \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string UPDATE_SUMMARY = "Updates an existing gateway";
        public const string UPDATE_DESCRIPTION = "* Requires admin privileges to the team \n * TeamId, name, firmwareId and url are required \n * Languages are: 0 (NA), 1 (JavaScript) \n * Returns a boolean that shows the action is done successfully or not \n * Attribute property can contains any additional data related to the gateway \n * FloorId is the floor that the gateway is located in \n * Security contains different parts for custom HTTP headers, defining to accept requests only over SSL or TLS, defining IP restriction and username and password for MQTT \n * In security section, Http.Headers should be like this, {'customKey': 'customValue'} \n * In security section, ipRestriction contains two list of Ips, one for Allowed ips and one for denied ips. It also accepts range Ips like 192.168.0.1-10 or 192.168.0.1/24 \n * Url should be unique in the namespace and should be alphanumeric  \n * X, Y are coordinates of gateway position on map based on percent. e.g. 20 and 20 means 20 percent down and 20 percent right from top left point  \n * Gateway url cannot start with momentaj and ubeac \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string DELETE_SUMMARY = "Deletes a gateway";
        public const string DELETE_DESCRIPTION = "* Requires admin privileges to that team \n * GatewayId is required \n * Returns a boolean that shows the action is done successfully or not \n * Deletes gateway data, sensor data and device summary related to that gateway";
        public const string GETDATA_SUMMARY = "Gets data related to the gateway";
        public const string GETDATA_DESCRIPTION = "* Requires view privileges to that team \n * GatewayId is required \n * Returns data order by date descending";
        public const string EXISTS_SUMMARY = "Checks gateway url existans in the team";
        public const string EXISTS_DESCRIPTION = "Checks if a gateway with this url exists in the team or not";
    }

    public class SensorConstants
    {
        public const string DESCRIPTION = "Sensor management";
        public const string ADD_SUMMARY = "Adds a new sensor";
        public const string ADD_DESCRIPTION = "* Requires admin privileges to the team \n * TeamId, deviceId, type, uid and name are required \n * Sensor uid should be unique in a device \n * Attribute property can contains any additional data related to the sensor \n * Returns id of the added sensor \n * Type is a digit which points to sensor type, please check https://ubeac.github.io/docs/SensorTypes.html \n * Unit is a digit which points to unit of data, please check https://ubeac.github.io/docs/SensorUnits.html \n * Prefix is a digit which points to prefix unit of data like kilo, mega,... , please check https://ubeac.github.io/docs/UnitPrefixes.html \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string UPDATE_SUMMARY = "Updates an existing sensor";
        public const string UPDATE_DESCRIPTION = "* Requires admin privileges to that team \n * TeamId, deviceId, type, uid and name are required \n * Sensor uid should be unique in a device \n * You cannot change sensor uid \n * Returns a boolean that shows the action is done successfully or not \n * Attribute property can contains any additional data related to the sensor \n * Type is a digit which points to sensor type, please check https://ubeac.github.io/docs/SensorTypes.html \n * Unit is a digit which points to unit of data, please check https://ubeac.github.io/docs/SensorUnits.html \n * Prefix is a digit which points to prefix unit of data like kilo, mega,... , please check https://ubeac.github.io/docs/UnitPrefixes.html \n * Name property should be between 2 to 150 characters \n * Description property should be maximum 500 characters";
        public const string DELETE_SUMMARY = "Deleting a sensor is not allowed";
        public const string DELETE_DESCRIPTION = "";
        public const string GETDATA_SUMMARY = "Gets data related to specific devices and sensors";
        public const string GETDATA_DESCRIPTION = "* Requires view privileges to that team \n * Returns data order by date descending \n * If you don't set from date and to date it will return data for last day \n * If you don't set gateway id, device id or sensor id it will set automatically";
    }

    public class FileConstants
    {
        public const string DESCRIPTION = "File management";
        public const string UPLOAD_SUMMARY = "Uploads a new file";
        public const string UPLOAD_DESCRIPTION = "* Requires to be logged in to panel \n * File size should be less than 3MB \n * User cannot upload executable files like dll, js, exe,...";
        public const string DOWNLOAD_SUMMARY = "Downloads an existing file";
        public const string DOWNLOAD_DESCRIPTION = "* Requires to be logged in to panel \n * FileId is required ";
    }
}
