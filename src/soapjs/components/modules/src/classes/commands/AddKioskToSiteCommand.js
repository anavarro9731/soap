import { validateArgs, types, ApiCommand } from '../../soap';

export class AddKioskToSiteCommand_c100v1 extends ApiCommand {
  constructor({ c100_siteId, c100_kiosk }) {
    validateArgs([{ c100_siteId }, types.string], [{ c100_kiosk }, Kiosk]);

    super();

    this.schema = 'AddKioskToSiteCommand_c100v1';

    this.c100_siteId = c100_siteId;
    this.c100_kiosk = c100_kiosk;
  }
}

class Kiosk {
  constructor({
    c100_kioskId,
    c100_kioskName,
    c100_ip,
    c100_ip2,
    c100_buildType,
    c100_buildVersion,
    c100_buildName,
    c100_cloudPrinterId,
    c100_paymentTerminalId,
    c100_kioskType,
    c100_tableNumbers,
    c100_approvedInstances,
  }) {
    validateArgs(
      [{ c100_kioskId }, types.string],
      [{ c100_kioskName }, types.string],
      [{ c100_ip }, types.string],
      [{ c100_ip2 }, types.string],
      [{ c100_buildType }, types.string],
      [{ c100_buildVersion }, types.string],
      [{ c100_buildName }, types.string],
      [{ c100_cloudPrinterId }, types.string],
      [{ c100_paymentTerminalId }, types.string],
      [{ c100_kioskType }, types.string],
      [{ c100_tableNumbers }, [types.number]],
      [{ c100_approvedInstances }, [KioskInstance]],
    );

    this.c100_kioskId = c100_kioskId;
    this.c100_kioskName = c100_kioskName;
    this.c100_ip = c100_ip;
    this.c100_ip2 = c100_ip2;
    this.c100_buildType = c100_buildType;
    this.c100_buildVersion = c100_buildVersion;
    this.c100_buildName = c100_buildName;
    this.c100_cloudPrinterId = c100_cloudPrinterId;
    this.c100_paymentTerminalId = c100_paymentTerminalId;
    this.c100_kioskType = c100_kioskType;
    this.c100_tableNumbers = c100_tableNumbers;
    this.c100_approvedInstances = c100_approvedInstances;
  }
}

class KioskInstance {
  constructor({ c100_appInstanceId, c100_deviceId }) {
    validateArgs(
      [{ c100_appInstanceId }, types.string],
      [{ c100_deviceId }, types.string],
    );

    this.c100_appInstanceId = c100_appInstanceId;
    this.c100_deviceId = c100_deviceId;
  }
}

AddKioskToSiteCommand_c100v1.Kiosk = Kiosk;
AddKioskToSiteCommand_c100v1.Kiosk.KioskInstance = KioskInstance;
