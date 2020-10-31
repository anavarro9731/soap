import { validateArgs, types, ApiEvent } from '../../soap';

export class GetSitesAndKiosksQueryReadyEvent_e118v1 extends ApiEvent {
  constructor({ e118_sites }) {
    validateArgs([{ e118_sites }, [Site]]);

    super();

    this.schema = 'GetSitesAndKiosksQueryReadyEvent_e118v1';

    this.e118_sites = e118_sites;
  }
}

class Site {
  constructor({ e118_siteId, e118_siteName, e118_kiosks }) {
    validateArgs(
      [{ e118_siteId }, types.string],
      [{ e118_siteName }, types.string],
      [{ e118_kiosks }, [Kiosk]],
    );

    this.e118_siteId = e118_siteId;
    this.e118_siteName = e118_siteName;
    this.e118_kiosks = e118_kiosks;
  }
}

class Kiosk {
  constructor({ e118_kioskId, e118_kioskName }) {
    validateArgs(
      [{ e118_kioskId }, types.string],
      [{ e118_kioskName }, types.string],
    );

    this.e118_kioskId = e118_kioskId;
    this.e118_kioskName = e118_kioskName;
  }
}

GetSitesAndKiosksQueryReadyEvent_e118v1.Site = Site;
GetSitesAndKiosksQueryReadyEvent_e118v1.Site.Kiosk = Kiosk;
