import { AngularBPWorkflowTemplatePage } from './app.po';

describe('AngularBPWorkflow App', function() {
  let page: AngularBPWorkflowTemplatePage;

  beforeEach(() => {
    page = new AngularBPWorkflowTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
