import { useCallback, useEffect, useState } from 'react';
import moment, { Moment } from 'moment';
import { 
  Layout, 
  Breadcrumb, 
  Row, 
  Col, 
  DatePicker, 
  Button,
  Space,
  Table,
  Popconfirm,
  Form,
  notification,
  Spin,
  Input
} from 'antd';
import { 
  ArrowLeftOutlined,
  EditFilled,
  DeleteFilled,
  LoadingOutlined
} from '@ant-design/icons';

import api from '../../services/api';

import Sider from '../../components/Sider';

export type FormInfoType = 'Adicionar' | 'Editar';

interface FormData {
  name: string;
  cpf: string;
  birthDate: Moment;
}

interface CustomerPagedResult {
  list: {
    id: number;
    name: string;
    cpf: {
      number: string;
    },
    birthDate: {
      date: string;
    };
  }[];
  totalResults: number;
  pageIndex: number;
  pageSize: number;
  query: string;
}

interface CustomerRowData {
  key: string;
  name: string;
  cpf: string;
  birthDate: string;
  id: number;
}

const { Content } = Layout;
const { Search } = Input;

const validateMessages = {
  required: '${label} é um item obrigatório!',
};

const Customers: React.FC = () => {
  const [searchText, setSearchText] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [currentPageSize, setCurrentPageSize] = useState(8);

  const [customerToEdit, setCustomerToEdit] = useState<CustomerRowData>();
  const [totalCustomers, setTotalCustomers] = useState(0);
  const [customersRowData, setCustomersRowData] = useState<CustomerRowData[]>();
  const [customersDataLoading, setCustomersDataLoading] = useState(false);

  const [formOpened, setFormOpened] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [formInfo, setFormInfo] = useState<FormInfoType>();
  
  const [rowKeyVisible, setRowKeyVisible] = useState('');
  const [rowKeyLoading, setRowKeyLoading] = useState('');

  const tableColumns = [
    {
      title: 'Nome',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'CPF',
      dataIndex: 'cpf',
      key: 'cpf',
    },
    {
      title: 'Nascimento',
      dataIndex: 'birthDate',
      key: 'birthDate',
    },
    {
      title: 'Ações',
      key: 'action',
      render: (customer: CustomerRowData) => (
        <Space size="middle">
          <Button 
            type="default" 
            icon={<EditFilled style={{ color: '#ffec3d' }} />} 
            size={'middle'}
            onClick={() => handleEditInitialization(customer)}
          />

          <Popconfirm
            title="Deseja remover esse cliente?"
            visible={rowKeyVisible == customer.key}
            onConfirm={() => handleDelete(customer)}
            okButtonProps={{ loading: rowKeyLoading == customer.key}}
            onCancel={() => setRowKeyVisible('')}
          >
            <Button 
              type="default" 
              icon={<DeleteFilled style={{ color: '#ff4d4f' }} />} 
              size={'middle'} 
              onClick={() => setRowKeyVisible(customer.key)}
            />
          </Popconfirm>
        </Space>
      ),
    },
  ];
  
  useEffect(() => {
    loadCustomersData();
  }, [searchText, currentPage, currentPageSize]);
  
  const loadCustomersData = useCallback(async () => {
    try {
      setCustomersDataLoading(true);

      const queryParams = 
        `ps=${currentPageSize}&page=${currentPage}&q=${searchText}`;
      
        const response = await api.get<CustomerPagedResult>(
        `api/customer/all?${queryParams}`
      );
  
      if (response) {
        setTotalCustomers(response.data.totalResults);
        setCustomersRowData(response.data.list.map((customer, index) => {
          const birthDate = new Date(customer.birthDate.date)
            .toISOString()
            .slice(0,10);
  
          return {
            key: index.toString(),
            name: customer.name,
            cpf: customer.cpf.number,
            birthDate,
            id: customer.id
          } as CustomerRowData;
        }));
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao buscar os clientes!'
      });
    } finally {
      setCustomersDataLoading(false);
    }
  }, [searchText, currentPage, currentPageSize]);

  const handleDelete = useCallback(async (customer: CustomerRowData) => {
    try {
      setRowKeyLoading(customer.key);
      const response = await api.delete(`api/customer/${customer.id}`);

      if (response) {
        loadCustomersData();
      }
    } catch (e) {
      notification.error({
        message: 'Erro ao remover cliente!'
      });
    } finally {
      setRowKeyLoading('');
    }
  }, [setRowKeyLoading]);

  const handleSearch = useCallback((value: string) => {
    setSearchText(value);
  }, []);

  const handlePageChange = useCallback((page: number, pageSize: number) => {
    setCurrentPage(page);
    setCurrentPageSize(pageSize);
  }, []);

  const handleSubmitForm = useCallback(async (values: FormData) => {
    try {
      setFormLoading(true);
      
      const birthDate = 
        new Date(values.birthDate.toDate()).toISOString().slice(0,10);
        
      const data = {
        name: values.name,
        cpf: values.cpf,
        birthDate,
      };
      
      const response = !customerToEdit 
        ? await api.post('api/customer', data)
        : await api.put(`api/customer/${customerToEdit.id}`, data);

      if (response) {
        const operation = !customerToEdit ? 'adicionado' : 'editado';
        notification.success({
          message: `Cliente ${operation} com sucesso!`,
        });

        setFormOpened(false);
        setFormInfo(undefined);
        loadCustomersData();
      }
    } catch (e) {
      const operation = !customerToEdit ? 'adicionar' : 'editar';
      notification.error({
        message: `Erro ao ${operation} cliente!`,
        description: 
          `Ocorreu um erro ao ${operation} o cliente! ` 
          + 'Verifique os dados de cadastro informados.'
      });
    } finally {
      setFormLoading(false);
    }
  }, [customerToEdit]);

  const handleEditInitialization = useCallback((customer: CustomerRowData) => {
    setCustomerToEdit(customer);
    setFormOpened(true);
    setFormInfo('Editar');
  }, []);

  const handleAddInitialization = useCallback(() => {
    setFormOpened(true);
    setFormInfo('Adicionar');
    setCustomerToEdit(undefined);
  }, []);


  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider />
      <Layout className="site-layout">
        <Content style={{ margin: '0 16px' }}>
          <Breadcrumb style={{ margin: '16px 0' }}>
            <Breadcrumb.Item>
              Clientes
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
                    <Search
                      placeholder={"Pesquise por nome do cliente..."}
                      onSearch={handleSearch}
                      style={{ minWidth: 320 }}
                      allowClear
                    />
                  </Col>
                  <Col span={3}/>
                  <Col span={1}>
                    <Spin spinning={customersDataLoading} indicator={
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
                  </Col>
                </Row>

                <Table 
                  columns={tableColumns} 
                  dataSource={customersRowData}
                  pagination={{
                    current: currentPage,
                    pageSize: currentPageSize,
                    total: totalCustomers,
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
                        name={'name'} 
                        label="Nome"
                        rules={[{ required: true }]}
                        initialValue={
                          customerToEdit ? customerToEdit.name : undefined
                        }
                      >
                        <Input placeholder="Informar nome do cliente" />
                      </Form.Item>
                    </Col>
                    
                    <Col span={24}>
                      <Form.Item 
                        name={'cpf'} 
                        label="CPF"
                        rules={[{ required: true }]}
                        initialValue={
                          customerToEdit ? customerToEdit.cpf : undefined
                        }
                      >
                        <Input placeholder="Informar CPF do cliente" />
                      </Form.Item>
                    </Col>

                    <Col span={24}>
                      <Form.Item 
                        name={'birthDate'} 
                        label="Nascimento"
                        rules={[{ required: true }]}
                        initialValue={
                          customerToEdit 
                            ? moment(customerToEdit.birthDate, 'YYYY-MM-DD') 
                            : undefined
                        }
                      >
                        <DatePicker style={{ width: '100%' }} />
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
                      {!customerToEdit ? 'Adicionar' : 'Salvar'}
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

export default Customers;
