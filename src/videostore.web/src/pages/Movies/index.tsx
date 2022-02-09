import { useCallback, useEffect, useState } from 'react';
import filesize from 'filesize';
import { 
  Layout, 
  Breadcrumb, 
  Row, 
  Col, 
  Button,
  Table,
  notification,
  Spin,
  Input,
  Modal
} from 'antd';
import { 
  DownloadOutlined,
  LoadingOutlined
} from '@ant-design/icons';

import api from '../../services/api';

import Sider from '../../components/Sider';

import Dropzone from '../../components/Dropzone';
import { FileInfo } from '../../components/Dropzone/styles';

interface MoviePagedResult {
  list: {
    id: number;
    launch: number;
    parentalRating: number;
    title: string;
  }[];
  totalResults: number;
  pageIndex: number;
  pageSize: number;
  query: string;
}

interface MovieRowData {
  key: string;
  title: string;
  parentalRating: string;
  launch: string;
}

interface FileProps {
  file: File;
  name: string;
  readableSize: string;
}

const { Content } = Layout;
const { Search } = Input;

const Movies: React.FC = () => {
  const [uploadedFile, setUploadedFile] = useState<FileProps>();
  const [importModalLoading, setImportModalLoading] = useState(false);
  const [importModalVisible, setImportModalVisible] = useState(false);

  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [currentPageSize, setCurrentPageSize] = useState(8);

  const [totalMovies, setTotalMovies] = useState(0);
  const [moviesRowData, setMoviesRowData] = useState<MovieRowData[]>();
  const [moviesDataLoading, setMoviesDataLoading] = useState(false);

  const tableColumns = [
    {
      title: 'Título',
      dataIndex: 'title',
      key: 'title',
    },
    {
      title: 'Classificação Indicativa',
      dataIndex: 'parentalRating',
      key: 'parentalRatings',
    },
    {
      title: 'Lançamento',
      dataIndex: 'launch',
      key: 'launch',
    },
  ];
  
  useEffect(() => {
    loadMoviesData();
  }, [searchText, currentPage, currentPageSize]);
  
  const loadMoviesData = useCallback(async () => {
    try {
      setMoviesDataLoading(true);

      const queryParams = 
        `ps=${currentPageSize}&page=${currentPage}&q=${searchText}`;
      
        const response = await api.get<MoviePagedResult>(
        `api/movie/all?${queryParams}`
      );
  
      if (response) {
        setTotalMovies(response.data.totalResults);
        setMoviesRowData(response.data.list.map((movie, index) => {
          return {
            key: index.toString(),
            title: movie.title,
            parentalRating: String(movie.parentalRating),
            launch: Boolean(movie.launch) ? 'Sim' : 'Não',
          } as MovieRowData;
        }));
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao buscar os filmes!'
      });
    } finally {
      setMoviesDataLoading(false);
    }
  }, [searchText, currentPage, currentPageSize]);

  const handleSearch = useCallback((value: string) => {
    setSearchText(value);
  }, []);

  const handlePageChange = useCallback((page: number, pageSize: number) => {
    setCurrentPage(page);
    setCurrentPageSize(pageSize);
  }, []);

  const handleImportData = useCallback(async () => {
    try {
      setImportModalLoading(true);

      const data = new FormData();

      if (!uploadedFile) return;

      data.append('csvFile', uploadedFile.file, uploadedFile.name);

      const response = await api.post('api/movie/import', data);

      if (response) {
        notification.success({
          message: 'Filmes importados com sucesso',
          placement: 'bottomRight',
        });

        setImportModalVisible(false);
        setUploadedFile(undefined);
        loadMoviesData();
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao importar filmes!',
        description: 'Ocorreu um erro! ' + 
          'Verifique se os filmes já foram importados.'
      });
    } finally {
      setImportModalLoading(false);
    }
  }, [uploadedFile]);

  const handleSubmitFile = useCallback((files: File[]) => {
    const file = files[0];

    if (file) {
      setUploadedFile({
        file,
        name: file.name,
        readableSize: filesize(file.size)
      });
    }
  }, []);

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider />
      <Layout className="site-layout">
        <Content style={{ margin: '0 16px' }}>
          <Breadcrumb style={{ margin: '16px 0' }}>
            <Breadcrumb.Item>
              Filmes
            </Breadcrumb.Item>
          </Breadcrumb>

          <div 
            className="site-layout-background" 
            style={{ padding: 24, minHeight: 360, background: "#fff" }}
          >
            <>
              <Row gutter={[8, 24]}>
                <Col span={10}>
                  <Search
                    placeholder={"Pesquise por título do filme..."}
                    onSearch={handleSearch}
                    style={{ minWidth: 320 }}
                    allowClear
                  />
                </Col>
                <Col span={3}/>
                <Col span={1}>
                  <Spin spinning={moviesDataLoading} indicator={
                    <LoadingOutlined style={{ fontSize: 24 }} spin />
                  } />
                </Col>
                <Col span={10}>
                  <Button 
                    type="ghost" 
                    icon={<DownloadOutlined />} 
                    style={{ minWidth: 240, float: "right" }}
                    onClick={() => setImportModalVisible(true)}
                  >
                    Importar dados
                  </Button>
                </Col>
              </Row>

              <Table 
                columns={tableColumns} 
                dataSource={moviesRowData}
                pagination={{
                  current: currentPage,
                  pageSize: currentPageSize,
                  total: totalMovies,
                  onChange: handlePageChange,
                }}
                style={{ marginTop: 24 }} 
              />
            </>
          </div>
        </Content>
      </Layout>
      
      <Modal
        title="Importar filmes com .csv"
        visible={importModalVisible}
        onOk={handleImportData}
        confirmLoading={importModalLoading}
        onCancel={() => {
          setImportModalVisible(false);
          setUploadedFile(undefined);
        }}
      >
        <div>
          <Dropzone onUpload={handleSubmitFile} />

          {uploadedFile && 
            <FileInfo style={{ marginTop: 12 }}>
              <div>
                <strong>{uploadedFile.name}</strong>
                <span>{uploadedFile.readableSize}</span>
              </div>
            </FileInfo>
          }
        </div>
      </Modal>
    </Layout>
  );

}

export default Movies;
