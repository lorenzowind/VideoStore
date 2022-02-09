import { useCallback, useEffect, useState } from 'react';
import moment, { Moment } from 'moment';
import FileSaver from 'file-saver';
import { 
  Layout, 
  Breadcrumb, 
  Row, 
  Col, 
  DatePicker, 
  Button,
  Space, 
  Tag, 
  Table,
  Popconfirm,
  Form,
  Select,
  Switch,
  Typography,
  notification,
  Spin
} from 'antd';
import { 
  ArrowLeftOutlined,
  DownloadOutlined,
  EditFilled,
  DeleteFilled,
  LoadingOutlined
} from '@ant-design/icons';

import api from '../../services/api';

import Sider from '../../components/Sider';

export type FormInfoType = 'Adicionar' | 'Editar';

interface CustomerDto {
  id: number;
  name: string;
}

interface MovieDto {
  id: number;
  title: string;
}

interface FormData {
  customer: string;
  movie: string;
  rentalDate: Moment;
  returnDate: Moment;
}

interface RentalPagedResult {
  list: {
    id: number;
    isLate: boolean;
    customer: {
      id: number;
      name: string;
    },
    movie: {
      id: number;
      title: string;
    },
    rentalDate: {
      date: string;
    };
    returnDate: {
      date: string;
    };
  }[];
  totalResults: number;
  pageIndex: number;
  pageSize: number;
  query: string;
}

interface RentalRowData {
  key: string;
  rentalDate: string;
  returnDate: string;
  customer: string;
  customerId: string;
  movie: string;
  movieId: string;
  id: number;
  isLate: boolean;
}

const { Content } = Layout;
const { Text } = Typography;
const { Option } = Select;

const validateMessages = {
  required: '${label} é um item obrigatório!',
};

const Rentals: React.FC = () => {
  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [currentPageSize, setCurrentPageSize] = useState(8);

  const [rentalToEdit, setRentalToEdit] = useState<RentalRowData>();
  const [totalRentals, setTotalRentals] = useState(0);
  const [rentalsRowData, setRentalsRowData] = useState<RentalRowData[]>();
  const [rentalsDataLoading, setRentalsDataLoading] = useState(false);

  const [customersDto, setCustomersDto] = useState<CustomerDto[]>([]);
  const [moviesDto, setMoviesDto] = useState<MovieDto[]>([]);
  const [formOpened, setFormOpened] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [formInfo, setFormInfo] = useState<FormInfoType>();

  const [rentalDateDisabled, setRentalDateDisabled] = useState(false);
  const [returnDateDisabled, setReturnDateDisabled] = useState(true);
  
  const [rowKeyVisible, setRowKeyVisible] = useState('');
  const [rowKeyLoading, setRowKeyLoading] = useState('');

  const tableColumns = [
    {
      title: 'Locação',
      dataIndex: 'rentalDate',
      key: 'rentalDate',
    },
    {
      title: 'Devolução',
      key: 'returnDate',
      dataIndex: 'returnDate',
      render: (returnDate: string, rental: RentalRowData) => (
        <>
          {rental.isLate 
            ? <Tag color={'volcano'}>EM ATRASO</Tag> 
            : (rental.returnDate 
              ? <Tag color={'green'}>{returnDate}</Tag>
              : <Tag color={'geekblue'}>LOCADO</Tag>)
          }
        </>
      ),
    },
    {
      title: 'Cliente',
      dataIndex: 'customer',
      key: 'customer',
    },
    {
      title: 'Filme',
      dataIndex: 'movie',
      key: 'movie',
    },
    {
      title: 'Ações',
      key: 'action',
      render: (rental: RentalRowData) => (
        <Space size="middle">
          <Button 
            type="default" 
            icon={<EditFilled style={{ color: '#ffec3d' }} />} 
            size={'middle'}
            onClick={() => handleEditInitialization(rental)}
          />

          <Popconfirm
            title="Deseja remover essa locação?"
            visible={rowKeyVisible == rental.key}
            onConfirm={() => handleDelete(rental)}
            okButtonProps={{ loading: rowKeyLoading == rental.key}}
            onCancel={() => setRowKeyVisible('')}
          >
            <Button 
              type="default" 
              icon={<DeleteFilled style={{ color: '#ff4d4f' }} />} 
              size={'middle'} 
              onClick={() => setRowKeyVisible(rental.key)}
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    loadRentalsData();
  }, [searchText, currentPage, currentPageSize]);

  const loadRentalsData = useCallback(async () => {
    try {
      setRentalsDataLoading(true);
      const response = await api.get<RentalPagedResult>(
        `api/rental/all?ps=${currentPageSize}&page=${currentPage}&q=${searchText}`
      );
  
      if (response) {
        setTotalRentals(response.data.totalResults);
        setRentalsRowData(response.data.list.map((rental, index) => {
          const rentalDate = new Date(rental.rentalDate.date)
            .toISOString()
            .slice(0,10);
          
          const returnDate = rental.returnDate 
            ? new Date(rental.returnDate.date).toISOString().slice(0,10) 
            : '';
  
          return {
            key: index.toString(),
            rentalDate,
            returnDate,
            customer: rental.customer.name,
            customerId: String(rental.customer.id),
            movie: rental.movie.title,
            movieId: String(rental.movie.id),
            isLate: rental.isLate,
            id: rental.id
          } as RentalRowData;
        }));
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao buscar as locações!'
      });
    } finally {
      setRentalsDataLoading(false);
    }
  }, [searchText, currentPage, currentPageSize]);

  const loadFormData = useCallback(async () => {
    try {
      const response = await Promise.all([
        api.get<CustomerDto[]>('api/customer/min-data'),
        api.get<MovieDto[]>('api/movie/min-data')
      ]);
  
      if (response) {
        setCustomersDto(response[0].data);
        setMoviesDto(response[1].data);
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao buscar os dados do cadastro!'
      });
    }
  }, []);

  const handleDelete = useCallback(async (rental: RentalRowData) => {
    try {
      setRowKeyLoading(rental.key);
      const response = await api.delete(`api/rental/${rental.id}`);

      if (response) {
        loadRentalsData();
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao remover locação!'
      });
    } finally {
      setRowKeyLoading('');
    }
  }, [setRowKeyLoading]);

  const handleSearch = useCallback((
    _date: Moment | null, 
    dateString: string
  ) => {
    setSearchText(dateString);
  }, []);

  const handlePageChange = useCallback((page: number, pageSize: number) => {
    setCurrentPage(page);
    setCurrentPageSize(pageSize);
  }, []);

  const handleSubmitForm = useCallback(async (values: FormData) => {
    try {
      setFormLoading(true);

      const rentalDate = !rentalDateDisabled
        ? new Date(values.rentalDate.toDate()).toISOString().slice(0,10)
        : new Date().toISOString().slice(0,10);

      const returnDate = !returnDateDisabled 
        ? new Date(values.returnDate.toDate()).toISOString().slice(0,10)
        : null;

      const data = {
        customerId: Number(values.customer),
        movieId: Number(values.movie),
        rentalDate,
        returnDate
      };
      
      const response = !rentalToEdit 
        ? await api.post('api/rental', data)
        : await api.put(`api/rental/${rentalToEdit.id}`, data);

      if (response) {
        notification.success({
          message: 
            `Locação ${!rentalToEdit ? 'adicionada' : 'editada'} com sucesso!`,
        });

        setFormOpened(false);
        setFormInfo(undefined);
        loadRentalsData();
      }
    } catch (e) {
      notification.error({
        message: `Erro ao ${!rentalToEdit ? 'adicionar' : 'editar'} locação!`,
        description: 
          'Ocorreu um erro ao locar o filme! Verifique se ele está disponível.'
      });
    } finally {
      setFormLoading(false);
    }
  }, [rentalDateDisabled, returnDateDisabled, rentalToEdit]);

  const handleEditInitialization = useCallback((rental: RentalRowData) => {
    setRentalToEdit(rental);
    setFormOpened(true);
    setFormInfo('Editar');
    setRentalDateDisabled(!rental.rentalDate);
    setReturnDateDisabled(!rental.returnDate);
    loadFormData();
  }, []);

  const handleAddInitialization = useCallback(() => {
    setFormOpened(true);
    setFormInfo('Adicionar');
    setRentalToEdit(undefined);
    setRentalDateDisabled(false);
    setReturnDateDisabled(true);
    loadFormData();
  }, []);

  const handleExportReport = useCallback(async () => {
    try {
      setRentalsDataLoading(true);
      const response = await api.get('api/rental/export', {
        responseType: 'blob'
      });

      if (response) {
        FileSaver.saveAs(response.data, `VideoStoreReport-${Date.now()}.xlsx`);

        notification.success({
          message: 'Relatório emitido com sucesso',
          description:
            'Faça o download do relatório e veja dados úteis do sistema.',
          placement: 'bottomRight',
        });
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao exportar relatório!'
      });
    } finally {
      setRentalsDataLoading(false);
    }
  }, []);

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider />
      <Layout className="site-layout">
        <Content style={{ margin: '0 16px' }}>
          <Breadcrumb style={{ margin: '16px 0' }}>
            <Breadcrumb.Item>
              Locações
            </Breadcrumb.Item>

            {formOpened && <Breadcrumb.Item>{formInfo}</Breadcrumb.Item> }
          </Breadcrumb>

          <div 
            className="site-layout-background" 
            style={{ padding: 24, minHeight: 360, background: "#fff" }}
          >
            {!formOpened 
              ? 
              <>
                <Row gutter={[8, 24]}>
                  <Col span={10}>
                    <DatePicker
                      placeholder={"Pesquise por data de locação..."}
                      onChange={handleSearch}
                      style={{ minWidth: 320 }}
                    />
                  </Col>
                  <Col span={3}/>
                  <Col span={1}>
                    <Spin spinning={rentalsDataLoading} indicator={
                      <LoadingOutlined style={{ fontSize: 24 }} spin />
                    } />
                  </Col>
                  <Col span={10}>
                    <Button 
                      type="primary" 
                      style={{ minWidth: 240, float: "right" }}
                      onClick={handleAddInitialization}
                    >
                      + Adicionar locação
                    </Button>
                    <Button 
                      type="ghost" 
                      icon={<DownloadOutlined />} 
                      style={{ minWidth: 240, float: "right", marginRight: 24 }}
                      onClick={handleExportReport}
                    >
                      Exportar relatório
                    </Button>
                  </Col>
                </Row>

                <Table 
                  columns={tableColumns} 
                  dataSource={rentalsRowData}
                  pagination={{
                    current: currentPage,
                    pageSize: currentPageSize,
                    total: totalRentals,
                    onChange: handlePageChange,
                  }}
                  style={{ marginTop: 24 }} 
                />
              </>
              : 
              <> 
                <Form 
                  layout="vertical" 
                  wrapperCol={{ span: 24 }} 
                  onFinish={handleSubmitForm} 
                  validateMessages={validateMessages}
                >
                  <Row gutter={24}>
                    <Col span={24}>
                      <Form.Item 
                        name={'customer'} 
                        label="Cliente"
                        rules={[{ required: true }]}
                        initialValue={
                          rentalToEdit ? rentalToEdit.customerId : undefined
                        }
                      >
                        <Select placeholder="Selecionar cliente">
                          {customersDto.map(c => (
                            <Option key={c.id}>{c.name}</Option>
                          ))}
                        </Select>
                      </Form.Item>
                    </Col>
                    
                    <Col span={24}>
                      <Form.Item 
                        name={'movie'} 
                        label="Filme"
                        rules={[{ required: true }]}
                        initialValue={
                          rentalToEdit ? rentalToEdit.movieId : undefined
                        }
                      >
                        <Select placeholder="Selecionar filme a ser alugado">
                          {moviesDto.map(m => (
                            <Option key={m.id}>{m.title}</Option>
                          ))}
                        </Select>
                      </Form.Item>
                    </Col>

                    <Col span={1}>
                      <Switch 
                        checked={rentalDateDisabled}
                        onChange={e => setRentalDateDisabled(e)}
                      />
                    </Col>
                    <Col span={5}><Text>Utilizar a data de hoje?</Text></Col>
                    <Col span={6}/>

                    <Col span={1}>
                      <Switch 
                        checked={!returnDateDisabled} 
                        onChange={e => setReturnDateDisabled(!e)}
                      />
                    </Col>
                    <Col span={5}><Text>Cliente realizou a entrega?</Text></Col>
                    <Col span={6}/>

                    <Col span={12} style={{ marginTop: 12 }}>
                      <Form.Item 
                        name={'rentalDate'} 
                        label="Locação"
                        rules={[{ required: !rentalDateDisabled }]}
                        initialValue={
                          rentalToEdit 
                            ? moment(rentalToEdit.rentalDate, 'YYYY-MM-DD') 
                            : undefined
                        }
                      >
                        <DatePicker
                          disabled={rentalDateDisabled}
                          style={{ width: '100%' }} 
                        />
                      </Form.Item>
                    </Col>

                    <Col span={12} style={{ marginTop: 12 }}>
                      <Form.Item 
                        name={'returnDate'} 
                        label="Devolução" 
                        rules={[{ required: !returnDateDisabled }]}
                        initialValue={
                          rentalToEdit 
                            ? (rentalToEdit.returnDate 
                              ? moment(rentalToEdit.returnDate, 'YYYY-MM-DD') 
                              : undefined) 
                            : undefined
                        }
                      >
                        <DatePicker 
                          disabled={returnDateDisabled} 
                          style={{ width: '100%' }} 
                        />
                      </Form.Item>
                    </Col>
                  </Row>

                  <Form.Item>
                    <Button
                      type="ghost" 
                      icon={<ArrowLeftOutlined />} 
                      size={'middle'} 
                      onClick={() => {
                        setFormOpened(false);
                        setFormInfo(undefined);
                      }}
                    >
                      Voltar
                    </Button>
                    
                    <Button 
                      type="primary" 
                      htmlType="submit" 
                      style={{ minWidth: 240, float: "right" }}
                    >
                      {!rentalToEdit ? 'Adicionar' : 'Salvar'}
                    </Button>

                    <Spin 
                      spinning={formLoading} 
                      indicator={
                        <LoadingOutlined style={{ fontSize: 24 }} spin />
                      }
                      style={{ marginRight: 24, float: "right" }}
                    />
                  </Form.Item>
                </Form>
              </>
            }
          </div>
        </Content>
      </Layout>
    </Layout>
  );
}

export default Rentals;
