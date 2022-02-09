import styled, { css, FlattenSimpleInterpolation } from 'styled-components';

interface UploadProps {
  isDragActive: boolean;
  isDragReject: boolean;
  refKey?: string;
  [key: string]: any;
  type?: 'error' | 'success' | 'default';
}

const dragActive = css`
  border-color: #73d13d;
`;

const dragReject = css`
  border-color: #ff4d4f;
`;

export const DropContainer = styled.div.attrs({ className: 'dropzone' })`
  border: 1.5px dashed #262626;
  border-radius: 5px;
  cursor: pointer;
  transition: height 0.2s ease;
  
  ${(props: UploadProps): false | FlattenSimpleInterpolation =>
    props.isDragActive && dragActive}
  
  ${(props: UploadProps): false | FlattenSimpleInterpolation =>
    props.isDragReject && dragReject}
`;

const messageColors = {
  default: '#262626',
  error: '#ff4d4f',
  success: '#73d13d',
};

export const UploadMessage = styled.p`
  display: flex;
  font-size: 16px;
  line-height: 24px;
  padding: 48px 0;
  color: ${({ type }: UploadProps) => messageColors[type || 'default']};
  justify-content: center;
  align-items: center;
`;

export const FileInfo = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex: 1;

  button {
    border: 0;
    background: transparent;
    color: #ff4d4f;
    margin-left: 5px;
    cursor: pointer;
  }
  
  div {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    
    span {
      font-size: 12px;
      color: #999;
      margin-top: 5px;
    }
  }
`;
