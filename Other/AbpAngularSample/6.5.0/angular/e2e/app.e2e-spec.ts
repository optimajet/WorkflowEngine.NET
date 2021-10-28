import { AbpAngularSampleTemplatePage } from './app.po';

describe('AbpAngularSample App', function() {
  let page: AbpAngularSampleTemplatePage;

  beforeEach(() => {
    page = new AbpAngularSampleTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
