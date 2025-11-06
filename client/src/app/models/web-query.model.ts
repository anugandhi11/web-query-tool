export interface WebQueryRequest {
  url: string;
  selector?: string;
  queryType: QueryType;
}

export enum QueryType {
  HtmlContent = 'HtmlContent',
  Text = 'Text',
  Links = 'Links',
  Images = 'Images',
  Metadata = 'Metadata'
}

export interface WebQueryResponse {
  success: boolean;
  content?: string;
  items?: string[];
  errorMessage?: string;
  metadata?: { [key: string]: string };
}
