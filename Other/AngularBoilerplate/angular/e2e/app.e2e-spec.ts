import { WorkflowTemplatePage } from './app.po';

describe('Workflow App', function() {
  let page: WorkflowTemplatePage;

  beforeEach(() => {
    page = new WorkflowTemplatePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
